output "project_id" {
  description = "Identifier of the Neon project created for the GameMarketIntel PoC."
  value       = neon_project.game_market_intel_poc.id
}

output "database_name" {
  description = "Name of the default Neon database."
  value       = neon_project.game_market_intel_poc.database_name
}

output "database_user" {
  description = "Name of the default Neon database role."
  value       = neon_project.game_market_intel_poc.database_user
}

output "database_host" {
  description = "Host of the direct Neon database connection."
  value       = neon_project.game_market_intel_poc.database_host
}

output "database_host_pooler" {
  description = "Host of the pooled Neon database connection."
  value       = neon_project.game_market_intel_poc.database_host_pooler
}

output "connection_uri" {
  description = "Direct Neon database connection string."
  value       = neon_project.game_market_intel_poc.connection_uri
  sensitive   = true
}

output "connection_uri_pooler" {
  description = "Pooled Neon database connection string."
  value       = neon_project.game_market_intel_poc.connection_uri_pooler
  sensitive   = true
}