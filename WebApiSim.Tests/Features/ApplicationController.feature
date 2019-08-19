Feature: ApplicationController
	Calling Application Controller

Background: set_headers
	Given the api is initialized
	And header test-app equals to 'WebApiSim.Specs'
	And header accept equals to 'application/json'

Scenario: application/clear_shoud_succeed
	When I send a POST request to api/application/clear
	Then the request should succeed

Scenario: application/applicationIds_should_return_the_existent_applicationIsd
	When I send a GET request to api/application/applicationIds
	Then the request should succeed
	And property data should be the single-element array
	|               |
	| application-1 |
	| application-2 |