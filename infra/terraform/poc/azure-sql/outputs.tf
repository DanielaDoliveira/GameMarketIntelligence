output "resource_group_name" {
  description = "Name of the Azure resource group used by the Azure SQL PoC."
  value       = azurerm_resource_group.poc.name
}

output "sql_server_name" {
  description = "Globally unique name of the Azure SQL logical server."
  value       = azurerm_mssql_server.poc.name
}

output "sql_server_fqdn" {
  description = "Fully qualified domain name of the Azure SQL logical server."
  value       = azurerm_mssql_server.poc.fully_qualified_domain_name
}

output "administrator_login" {
  description = "Administrator login of the Azure SQL logical server."
  value       = azurerm_mssql_server.poc.administrator_login
}

output "administrator_password" {
  description = "Generated administrator password of the Azure SQL logical server."
  value       = random_password.sql_admin_password.result
  sensitive   = true
}