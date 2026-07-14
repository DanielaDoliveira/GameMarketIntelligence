variable "render_api_key" {
  description = "Render API key used by Terraform."
  type        = string
  sensitive   = true
}

variable "render_owner_id" {
  description = "Render workspace or account owner ID."
  type        = string
}
variable "neon_connection_string" {
  description = "Neon PostgreSQL connection string used by the API."
  type        = string
  sensitive   = true
}