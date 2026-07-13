resource "random_string" "sql_server_suffix" {
  length  = 8
  special = false
  upper   = false
}

resource "random_password" "sql_admin_password" {
  length           = 24
  special          = true
  override_special = "!#$%&*()-_=+[]{}"
}

resource "azurerm_resource_group" "poc" {
  name     = var.resource_group_name
  location = var.location

  tags = {
    project     = "GameMarketIntel"
    environment = "poc"
    managed_by  = "Terraform"
  }
}

resource "azurerm_mssql_server" "poc" {
  name = "${var.sql_server_name_prefix}-${random_string.sql_server_suffix.result}"

  resource_group_name          = azurerm_resource_group.poc.name
  location                     = var.sql_location
  version                      = "12.0"
  administrator_login          = var.administrator_login
  administrator_login_password = random_password.sql_admin_password.result

  minimum_tls_version = "1.2"

  tags = {
    project     = "GameMarketIntel"
    environment = "poc"
    managed_by  = "Terraform"
  }
}