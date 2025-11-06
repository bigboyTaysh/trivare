#!/bin/bash

# Trivare Test Runner Script
# This script runs different test suites for the Trivare application
#
# Usage:
#   ./test.sh unit        - Run unit tests only
#   ./test.sh e2e         - Run E2E tests only
#   ./test.sh e2e:debug   - Run E2E tests in debug mode
#   ./test.sh all         - Run all tests (default)

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Global variables for process IDs
API_PID=""
CLIENT_PID=""
# Global flag to indicate if script should exit
INTERRUPTED=false

# Function to print status
print_status() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

print_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Load environment variables from .env file if it exists
if [ -f ".env" ]; then
    # Export variables one by one to handle connection strings properly
    while IFS='=' read -r key value; do
        if [[ $key =~ ^[[:space:]]*# ]] || [[ -z $key ]]; then
            continue
        fi
        # Use envsubst to expand variables in the value
        expanded_value=$(echo "$value" | envsubst)
        export "$key=$expanded_value"
    done < .env
fi

# Function to cleanup background processes
cleanup() {
    INTERRUPTED=true
    print_status "Cleaning up background processes..."
    if [ ! -z "$API_PID" ]; then
        kill $API_PID 2>/dev/null || true
        print_status "Stopped API server (PID: $API_PID)"
        API_PID=""
    fi
    if [ ! -z "$CLIENT_PID" ]; then
        kill $CLIENT_PID 2>/dev/null || true
        print_status "Stopped frontend dev server (PID: $CLIENT_PID)"
        CLIENT_PID=""
    fi
    # Kill any remaining processes that might have been started by this script
    pkill -f "dotnet run.*Api.csproj" || true
    pkill -f "npm run dev" || true
    # Also kill any processes listening on our test ports
    lsof -ti :6000 | xargs kill -9 2>/dev/null || true
    lsof -ti :4321 | xargs kill -9 2>/dev/null || true
    # Stop test database if it's running and Docker is available
    if docker info >/dev/null 2>&1 && docker-compose --profile test ps 2>/dev/null | grep -q "sqlserver-test"; then
        print_status "Stopping test database..."
        docker-compose --profile test down sqlserver-test || true
    fi
    print_status "Cleanup completed."
}

# Function to cleanup before starting services
pre_cleanup() {
    print_status "Performing pre-test cleanup..."
    # Kill any processes that might be using our ports
    lsof -ti :6000 | xargs kill -9 2>/dev/null || true
    lsof -ti :4321 | xargs kill -9 2>/dev/null || true
    pkill -f "dotnet run.*Api.csproj" || true
    pkill -f "npm run dev" || true
    sleep 2  # Give processes time to fully terminate
    print_status "Pre-test cleanup completed."
}


# Function to start test database
start_test_database() {
    print_status "Starting test database..."

    # Check if Docker is running
    if ! docker info >/dev/null 2>&1; then
        print_error "Docker is not running. E2E tests require Docker to run the isolated test database."
        print_error "Cannot continue with E2E tests as they would use the development/production database."
        print_error "Please start Docker and try again, or run unit tests only with: ./test.sh unit"
        return 1
    fi

    if ! docker-compose --profile test ps | grep -q "sqlserver-test"; then
        print_status "Starting sqlserver-test container..."
        if ! docker-compose --profile test up -d sqlserver-test; then
            print_error "Failed to start test database. Please check Docker is running and try again."
            exit 1
        fi

        print_status "Waiting for test database to be ready..."
        # Wait for the database to be accessible and create the test database
        local max_attempts=60  # 60 attempts * 5 seconds = 5 minutes max wait
        local attempt=1

        while [ $attempt -le $max_attempts ] && [ "$INTERRUPTED" = false ]; do
            print_status "Testing database connection (attempt $attempt)..."
            if docker-compose --profile test exec -T sqlserver-test /opt/mssql-tools18/bin/sqlcmd \
                -S localhost -U sa -P "$DB_PASSWORD" -C \
                -Q "SELECT 1;" 2>/dev/null | grep -q "1"; then
                print_success "Basic database connectivity verified!"

                # Now create the test database if it doesn't exist
                if docker-compose --profile test exec -T sqlserver-test /opt/mssql-tools18/bin/sqlcmd \
                    -S localhost -U sa -P "$DB_PASSWORD" -C \
                    -Q "IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'TrivareTest') CREATE DATABASE [TrivareTest];" >/dev/null 2>&1; then
                    print_success "TrivareTest database ready!"
                    break
                else
                    print_status "Database exists but couldn't create TrivareTest database..."
                fi
            else
                print_status "Database not ready yet, waiting..."
            fi

            sleep 5
            attempt=$((attempt + 1))
        done

        if [ "$INTERRUPTED" = true ]; then
            print_error "Database setup interrupted by user."
            exit 1
        fi
        if [ $attempt -gt $max_attempts ]; then
            print_error "Failed to connect to test database within 5 minutes."
            print_error "Container status:"
            docker-compose --profile test ps sqlserver-test
            print_error "Container logs:"
            docker-compose --profile test logs sqlserver-test | tail -30
            exit 1
        fi
    else
        print_status "Test database container is already running"
        # Verify database connectivity for existing container
        print_status "Verifying database connectivity for existing container..."
        local max_attempts=30
        attempt=1
        while [ $attempt -le $max_attempts ] && [ "$INTERRUPTED" = false ]; do
            if docker-compose --profile test exec -T sqlserver-test /opt/mssql-tools18/bin/sqlcmd \
                -S localhost -U sa -P "$DB_PASSWORD" -C \
                -Q "IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'TrivareTest') CREATE DATABASE [TrivareTest]; SELECT 1;" >/dev/null 2>&1; then
                print_success "Database connectivity verified for existing container!"
                break
            fi
            print_status "Testing database connectivity for existing container... (attempt $attempt/$max_attempts)"
            sleep 3
            attempt=$((attempt + 1))
        done
        if [ "$INTERRUPTED" = true ]; then
            print_error "Database setup interrupted by user."
            exit 1
        fi
        if [ $attempt -gt $max_attempts ]; then
            print_error "Failed to connect to existing test database container. Restarting..."
            docker-compose --profile test restart sqlserver-test
            print_status "Waiting for restarted database to be ready..."
            attempt=1
            while [ $attempt -le $max_attempts ] && [ "$INTERRUPTED" = false ]; do
                if docker-compose --profile test exec -T sqlserver-test /opt/mssql-tools18/bin/sqlcmd \
                    -S localhost -U sa -P "$DB_PASSWORD" -C \
                    -Q "IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'TrivareTest') CREATE DATABASE [TrivareTest]; SELECT 1;" >/dev/null 2>&1; then
                    print_success "Database connectivity verified after restart!"
                    break
                fi
                print_status "Waiting for restarted database... (attempt $attempt/$max_attempts)"
                sleep 5
                attempt=$((attempt + 1))
            done
            if [ "$INTERRUPTED" = true ]; then
                print_error "Database setup interrupted by user."
                exit 1
            fi
            if [ $attempt -gt $max_attempts ]; then
                print_error "Failed to connect to restarted test database container."
                exit 1
            fi
        fi
    fi
}

# Function to start backend API service
start_api() {
    local env=${1:-"Development"}
    print_status "Starting backend API service (Environment: $env)..."
    cd Server/Api
    ASPNETCORE_ENVIRONMENT=$env DOTNET_LAUNCH_PROFILE="" dotnet build --configuration Release --no-restore > /dev/null 2>&1
    ASPNETCORE_ENVIRONMENT=$env dotnet run --configuration Release --no-build --no-launch-profile > /dev/null &
    API_PID=$!
    cd ../..
    print_status "API server started (PID: $API_PID)"
}

# Function to start frontend dev server
start_frontend() {
    print_status "Starting frontend dev server..."
    cd Client
    npm install > /dev/null 2>&1
    npm run dev > /dev/null &
    CLIENT_PID=$!
    cd ..
    print_status "Frontend dev server started (PID: $CLIENT_PID)"
}

# Function to wait for services to be ready
wait_for_services() {
    if [ ! -z "$API_PID" ]; then
        print_status "Waiting for backend API to be ready..."
        sleep 10
    fi
    if [ ! -z "$CLIENT_PID" ]; then
        print_status "Waiting for frontend dev server to be ready..."
        sleep 10
    fi
}

# Function to run unit tests
run_unit_tests() {
    print_status "Running unit tests..."
    cd Server

    print_status "Running Domain unit tests..."
    if dotnet test Domain.Tests/Domain.Tests.csproj --logger "trx" --no-build > /dev/null; then
        print_success "Domain tests passed ✅"
    else
        print_error "Domain tests failed ❌"
        exit 1
    fi

    print_status "Running Application unit tests..."
    if dotnet test Application.Tests/Application.Tests.csproj --logger "trx" --no-build > /dev/null; then
        print_success "Application tests passed ✅"
    else
        print_error "Application tests failed ❌"
        exit 1
    fi

    print_status "Running Infrastructure unit tests..."
    if dotnet test Infrastructure.Tests/Infrastructure.Tests.csproj --logger "trx" --no-build > /dev/null; then
        print_success "Infrastructure tests passed ✅"
    else
        print_error "Infrastructure tests failed ❌"
        exit 1
    fi

    cd ..

    print_status "Running frontend unit tests..."
    cd Client

    print_status "Installing frontend dependencies..."
    npm install

    print_status "Running Vitest unit tests..."
    npm run test:run

    cd ..

    print_success "Unit tests completed successfully! ✅"
}

# Function to run E2E tests
run_e2e_tests() {
    local debug_mode=$1
    if [ "$debug_mode" = "debug" ]; then
        print_status "Running E2E tests in debug mode..."
        test_command="npm run test:e2e:debug"
    else
        print_status "Running E2E tests..."
        test_command="npm run test:e2e"
    fi

    # Pre-cleanup to ensure no leftover processes
    pre_cleanup

    # Start test database for E2E tests
    if start_test_database; then
        DB_AVAILABLE=true
        start_api "Testing"
    else
        print_error "Test database is not available. E2E tests require a test database to avoid using development/production databases."
        print_error "Please ensure Docker is running and try again, or run unit tests only with: ./test.sh unit"
        cleanup
        exit 1
    fi
    start_frontend
    wait_for_services


    # Run E2E tests
    echo ""
    echo "========================================"
    echo "🎭 RUNNING PLAYWRIGHT E2E TESTS"
    echo "========================================"
    cd Client
    eval $test_command
    E2E_EXIT_CODE=$?
    cd ..
    echo "========================================"

    # Always cleanup services after E2E tests
    cleanup

    # Display E2E test results prominently
    if [ $E2E_EXIT_CODE -eq 0 ]; then
        echo ""
        print_success "🎉 PLAYWRIGHT E2E TESTS PASSED! ✅"
        echo ""
    else
        echo ""
        print_error "❌ PLAYWRIGHT E2E TESTS FAILED! ❌ with exit code: $E2E_EXIT_CODE"
        exit $E2E_EXIT_CODE
    fi
}

# Function to run all tests
run_all_tests() {
    print_status "Running all tests..."

    # Pre-cleanup to ensure no leftover processes
    pre_cleanup

    # Start test database for E2E tests
    if start_test_database; then
        DB_AVAILABLE=true
        start_api "Testing"
    else
        print_error "Test database is not available. Cannot run E2E tests which require a test database to avoid using development/production databases."
        print_error "Running unit tests only..."
        run_unit_tests
        cleanup
        exit 1
    fi
    start_frontend
    wait_for_services

    # Run unit tests
    run_unit_tests


    # Run E2E tests
    echo ""
    echo "========================================"
    echo "🎭 RUNNING PLAYWRIGHT E2E TESTS"
    echo "========================================"
    cd Client
    npm run test:e2e
    E2E_EXIT_CODE=$?
    cd ..
    echo "========================================"

    # Cleanup all services
    cleanup

    # Display E2E test results prominently
    if [ $E2E_EXIT_CODE -eq 0 ]; then
        echo ""
        echo "🎉 PLAYWRIGHT E2E TESTS PASSED! ✅"
        echo "=================================================================================="
        print_success "All tests completed successfully! 🎉"
    else
        echo ""
        echo "❌ PLAYWRIGHT E2E TESTS FAILED! ❌"
        echo "=================================================================================="
        print_error "E2E tests failed with exit code: $E2E_EXIT_CODE"
        exit $E2E_EXIT_CODE
    fi
}

# Check if we're in the right directory
if [ ! -d "Client" ] || [ ! -d "Server" ]; then
    print_error "Please run this script from the project root directory"
    exit 1
fi

# Check prerequisites
if ! command -v dotnet &> /dev/null; then
    print_error ".NET SDK is not installed. Please install .NET 9 SDK."
    exit 1
fi

if ! command -v npm &> /dev/null; then
    print_error "npm is not installed. Please install Node.js."
    exit 1
fi

# Set environment variables for testing

# Parse command line argument
TEST_TYPE=${1:-"all"}

# Parse e2e:debug format
E2E_MODE=""
if [[ $TEST_TYPE == e2e:* ]]; then
    E2E_MODE=$(echo $TEST_TYPE | cut -d: -f2)
    TEST_TYPE="e2e"
fi

echo "🚀 Starting Trivare Test Suite ($TEST_TYPE${E2E_MODE:+:$E2E_MODE})"
echo "========================================"
echo ""

# Set trap to cleanup on exit and interrupt (for any tests that start services)
if [ "$TEST_TYPE" = "e2e" ] || [ "$TEST_TYPE" = "all" ]; then
    trap cleanup EXIT INT TERM
fi

# Run the appropriate test suite
case $TEST_TYPE in
    "unit")
        run_unit_tests
        ;;
    "e2e")
        run_e2e_tests $E2E_MODE
        ;;
    "all")
        run_all_tests
        ;;
    *)
        print_error "Invalid test type: $TEST_TYPE${E2E_MODE:+:$E2E_MODE}"
        print_error "Usage: ./test.sh [unit|e2e|e2e:debug|all]"
        exit 1
        ;;
esac

print_status "Test reports:"
print_status "- Backend unit tests: Server/TestResults/"
print_status "- Frontend unit tests: Client/coverage/"
print_status "- E2E tests: Client/playwright-report/"
