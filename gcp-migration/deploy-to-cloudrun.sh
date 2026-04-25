#!/bin/bash

################################################################################
# Backend Deployment Script - GCP Cloud Run
# Builds and deploys .NET API to Cloud Run
################################################################################

set -e  # Exit on any error

echo "========================================="
echo "Backend Deployment to GCP Cloud Run"
echo "========================================="

# Configuration
PROJECT_ID="flamemitra-prod"
REGION="asia-south1"
SERVICE_NAME="flamemitra-api"
IMAGE_NAME="gcr.io/$PROJECT_ID/$SERVICE_NAME"

# IMPORTANT: Update this before running!
# Get the connection string from GCP Secret Manager or set it here
DB_CONNECTION_STRING=""  # UPDATE BEFORE RUNNING - Example: "Server=34.100.216.87,1433;Database=YourDB;User Id=sa;Password=YourPassword;TrustServerCertificate=True"

if [ -z "$DB_CONNECTION_STRING" ]; then
  echo "❌ ERROR: DB_CONNECTION_STRING is not set!"
  echo "Please edit this script and set the database connection string."
  echo "Or pass it as an environment variable: export DB_CONNECTION_STRING='...'"
  exit 1
fi

# Set GCP project
echo "🔧 Setting GCP project to $PROJECT_ID..."
gcloud config set project "$PROJECT_ID"

# Navigate to backend directory
echo "📂 Navigating to WebAPI folder..."
cd "$(dirname "$0")/../../WebAPI"

# Build Docker image using Cloud Build
echo "🔨 Building Docker image with Cloud Build..."
gcloud builds submit --tag "$IMAGE_NAME" .

# Deploy to Cloud Run
echo "☁️  Deploying to Cloud Run..."
gcloud run deploy "$SERVICE_NAME" \
  --image "$IMAGE_NAME" \
  --platform managed \
  --region "$REGION" \
  --allow-unauthenticated \
  --min-instances 1 \
  --max-instances 5 \
  --memory 512Mi \
  --cpu 1 \
  --port 8080 \
  --set-env-vars "ASPNETCORE_ENVIRONMENT=Production,ConnectionStrings__DefaultConnection=$DB_CONNECTION_STRING"

# Get the service URL
SERVICE_URL=$(gcloud run services describe "$SERVICE_NAME" --region "$REGION" --format='value(status.url)')

echo "========================================="
echo "✅ Deployment Successful!"
echo "========================================="
echo ""
echo "🌐 Cloud Run Service URL: $SERVICE_URL"
echo "🌐 Public API URL: https://api.flamemitra.in (after DNS setup)"
echo ""
echo "Next steps:"
echo "1. Test the API: curl $SERVICE_URL/api/health"
echo "2. Configure DNS for api.flamemitra.in to point to Cloud Run"
echo "3. Set up domain mapping in Cloud Run console"
echo "========================================="
