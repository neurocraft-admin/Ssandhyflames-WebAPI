#!/bin/bash

################################################################################
# Backend Deployment Script - GCP Cloud Run
# Builds and deploys .NET API to Cloud Run
#
# CONNECTION STRING STRATEGY:
#   - Do NOT hardcode the password here.
#   - Set it once in Cloud Run via GCP Console or the one-time setup command below.
#   - This script builds & deploys WITHOUT touching the connection string env var,
#     so the value already stored in Cloud Run is preserved across deployments.
#
# ONE-TIME SETUP (run this once manually, never again):
#   gcloud run services update flamemitra-api \
#     --region=asia-south1 \
#     --project=project-2d8a14e5-82f9-4683-bba \
#     --update-env-vars="ConnectionStrings__DefaultConnection=Server=34.100.216.87,1433;Database=sandhyaflames;User Id=sa;Password=YOUR_PASSWORD;TrustServerCertificate=True"
#
# After that one-time setup, just run this script normally for all future deploys.
################################################################################

set -e  # Exit on any error

echo "========================================="
echo "Backend Deployment to GCP Cloud Run"
echo "========================================="

# Configuration
PROJECT_ID="project-2d8a14e5-82f9-4683-bba"
REGION="asia-south1"
SERVICE_NAME="flamemitra-api"
IMAGE_NAME="gcr.io/$PROJECT_ID/$SERVICE_NAME"

# Set GCP project
echo "🔧 Setting GCP project to $PROJECT_ID..."
gcloud config set project "$PROJECT_ID"

# Navigate to backend directory (script lives in gcp-migration/ subfolder)
echo "📂 Navigating to WebAPI folder..."
cd "$(dirname "$0")/../"

# Verify we are in the right folder
if [ ! -f "WebAPI.csproj" ]; then
  echo "❌ Error: WebAPI.csproj not found."
  echo "Expected folder structure: WebAPI/gcp-migration/deploy-to-cloudrun.sh"
  exit 1
fi

# Build Docker image using Cloud Build (no password involved here)
echo "🔨 Building Docker image with Cloud Build..."
gcloud builds submit --tag "$IMAGE_NAME" .

# Deploy to Cloud Run
# NOTE: --update-env-vars only sets ASPNETCORE_ENVIRONMENT.
#       ConnectionStrings__DefaultConnection is intentionally NOT set here —
#       it is managed separately via the one-time setup command above,
#       so the password never appears in this script or deployment logs.
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
  --update-env-vars "ASPNETCORE_ENVIRONMENT=Production"

# Get the service URL
SERVICE_URL=$(gcloud run services describe "$SERVICE_NAME" \
  --region "$REGION" \
  --format='value(status.url)')

echo "========================================="
echo "✅ Deployment Successful!"
echo "========================================="
echo ""
echo "🌐 Cloud Run Service URL: $SERVICE_URL"
echo "🌐 Public API URL: https://api.flamemitra.in"
echo ""
echo "🧪 Test health endpoint:"
echo "   curl $SERVICE_URL/api/health"
echo "   curl https://api.flamemitra.in/api/health"
echo ""
echo "⚠️  Reminder: If this is a fresh Cloud Run service (never deployed before),"
echo "   run the one-time connection string setup at the top of this script."
echo "========================================="