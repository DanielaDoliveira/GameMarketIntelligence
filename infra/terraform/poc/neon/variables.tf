variable "project_name" {
  description = "Name of the Neon project used by the GameMarketIntel PoC."
  type        = string
  default     = "gamemarketintel-poc"
}

variable "organization_id" {
  description = "Neon organization identifier where the project will be created."
  type        = string
}

variable "region_id" {
  description = "Neon region where the project will be created."
  type        = string
  default     = "aws-us-east-1"
}

variable "postgres_version" {
  description = "PostgreSQL major version used by the Neon project."
  type        = number
  default     = 17
}