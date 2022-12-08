.PHONY: format
format: ## format all files
	@dotnet format --report format-report.json

.PHONY: format-info
format-info: ## format all files with 'info' severity
	@dotnet format --report format-report.json --severity info

.PHONY: format-verify-no-changes
format-verify-no-changes: ## check for formatting problems
	@dotnet format --report format-report.json --verify-no-changes

.PHONY: print-format-report
print-format-report: ## print file paths and changes from format-report.json
	@jq '.[] | "\(.FilePath)(\(.FileChanges[0].LineNumber),\(.FileChanges[0].CharNumber))","\(.FileChanges[0].FormatDescription)","---"' -r format-report.json

.PHONY: print-format-report-files
print-format-report-files: ## print file paths from format-report.json
	@jq '.[] | .FilePath + "(" + (.FileChanges[0].LineNumber|tostring) + "," + (.FileChanges[0].CharNumber|tostring) + ")"' -r format-report.json

.PHONY: test
test: ## run unit tests
	@dotnet test --logger "console;verbosity=detailed"

.PHONY: test-coverage
test-coverage: ## run tests with code coverage
	@dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults

.PHONY: test-coverage-txt
test-coverage-txt: test-coverage ## run tests with code coverage and output a report
	@reportgenerator "-reports:./TestResults/*/coverage.cobertura.xml" "-targetdir:./TestResults" "-reporttypes:TextSummary"
	@cat TestResults/Summary.txt

.PHONY: test-coverage-html
test-coverage-html: test-coverage ## run tests with code coverage and generate an html report
	@reportgenerator "-reports:./TestResults/*/coverage.cobertura.xml" "-targetdir:./TestResults"
ifdef OPEN
	@$(OPEN) TestResults/index.html > /dev/null 2>&1
endif

# Ignoring errors - this command returns a non-zero exit code if the tool is already installed
.PHONY: reportgenerator-install
reportgenerator-install: ## install dotnet-reportgenerator-globaltool
	@dotnet tool install -g dotnet-reportgenerator-globaltool || true

.PHONY: test-coverage-ci
test-coverage-ci: reportgenerator-install ## run tests with code coverage and generate reports for ci
	@dotnet test --collect:"XPlat Code Coverage" -r ./TestResults --logger GitHubActions
	@reportgenerator "-reports:./TestResults/*/coverage.cobertura.xml" "-targetdir:./TestResults" "-reporttypes:JsonSummary;TextSummary"
