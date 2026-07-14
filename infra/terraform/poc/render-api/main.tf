resource "render_web_service" "api" {
  name   = "gamemarketintel-api-poc"
  plan   = "free"
  region = "oregon"

  runtime_source = {
    docker = {
      repo_url        = "https://github.com/DanielaDoliveira/GameMarketIntelligence"
      branch          = "main"
      auto_deploy     = true
      dockerfile_path = "./Dockerfile"
      context         = "."
    }
  }

  env_vars = {
    "ConnectionStrings__DefaultConnection" = {
      value = var.neon_connection_string
    }

    "ASPNETCORE_ENVIRONMENT" = {
      value = "Production"
    }
  }
}