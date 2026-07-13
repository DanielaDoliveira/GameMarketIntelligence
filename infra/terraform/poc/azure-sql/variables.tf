variable "resource_group_name" {
  description = "Name of the Azure resource group used by the GameMarketIntel Azure SQL PoC."
  type        = string
  default     = "rg-gamemarketintel-sql-poc"
}

variable "location" {
  description = "Azure region where the PoC resources will be created."
  type        = string
  default     = "East US"
}

variable "sql_location" {
  description = "Azure region where the Azure SQL logical server will be created."
  type        = string
  default     = "Central US"
}

variable "sql_server_name_prefix" {
  description = "Prefix used to generate the globally unique Azure SQL logical server name."
  type        = string
  default     = "gamemarketintel-sql-poc"
}

variable "database_name" {
  description = "Name of the Azure SQL database used by the GameMarketIntel PoC."
  type        = string
  default     = "gamemarketintel"
}

variable "administrator_login" {
  description = "Administrator login used by the Azure SQL logical server."
  type        = string
  default     = "gamemarketinteladmin"
}