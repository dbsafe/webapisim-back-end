Feature: ResponseController
	Calling Response Controller

Background: set_headers
	Given the api is initialized
	And header test-app equals to 'WebApiSim.Specs'
	And header accept equals to 'application/json'

Scenario: response/responsesByApplicationId_when_the_application_not_exists_should_return_an_empty_array
	Given property applicationId equals to 'non-existent-application'
	When I send a POST request to api/response/responsesByApplicationId
	Then the request should succeed
	And property data should be an empty array

Scenario: response/responsesByApplicationId_should_return_the_responses
	Given property applicationId equals to 'application-1'
	When I send a POST request to api/response/responsesByApplicationId
	Then the request should succeed

	#And property data should be an array with 2 items
	
	And property data[0].responseId should be '00000000-0000-0000-0001-000000000001'
	And property data[0].statusCode should be the number 200
	And property data[0].body.name should be 'Mario Perez'
	And property data[0].body.dob should be '2000-10-11'
	And property data[0].headers should be an array with 2 items
	And property data[0].headers[0].key should be 'Content-Type'
	And property data[0].headers[0].value should be an array with 1 item
	And property data[0].headers[0].value[0] should be 'application/json'
	
	And property data[1].responseId should be '00000000-0000-0000-0001-000000000002'
	And property data[1].statusCode should be the number 500

Scenario: response/clearResponses_shoud_succeed
	Given property applicationId equals to 'application-1'
	When I send a POST request to api/response/clear
	Then the request should succeed
