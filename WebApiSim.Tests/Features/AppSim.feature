Feature: AppSimLogic
	Test the API Simulator logic

Background: set_headers
	Given the api is initialized

Scenario: ApiSim_when_applicationId_is_not_found_must_return_a_default_response
	When I send a GET request to api-sim/invalid-app-id
	Then StatusCode should be 200
	And header simulated-response-type should be 'application-not-found'
