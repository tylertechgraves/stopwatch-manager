.PHONY: format-verify-no-changes
format-verify-no-changes: ## check for formatting problems
	@dotnet format --report format-report.json --verify-no-changes

# Run format twice. The first run is to fix issues that can be corrected automatically, the second is to report
# if there are issues that need a dev.
.PHONY: format-auto-fix
format-auto-fix: ## check and fix formatting problems
	@dotnet format --report format-report.json
	@dotnet format --report format-report.json --verify-no-changes

#.PHONY: format-sdk-verify-no-changes
#format-sdk-verify-no-changes: ## check for sdk formatting problems	
#	@dotnet format --report format-report.sdk.json --verify-no-changes ./path/to/sdk/