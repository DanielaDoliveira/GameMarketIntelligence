resource "neon_project" "game_market_intel_poc" {
  name       = var.project_name
  org_id     = var.organization_id
  region_id  = var.region_id
  pg_version = var.postgres_version

  history_retention_seconds = 21600

  branch {
    name          = "production"
    database_name = "gamemarketintel"
    role_name     = "gamemarketintel_owner"
  }

  default_endpoint_settings {
    autoscaling_limit_min_cu = 0.25
    autoscaling_limit_max_cu = 1.0
  }
}