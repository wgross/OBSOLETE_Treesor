Feature: ValueManagement
	As a web client
	I want to store, read and remove values with a hierarchical key

@mytag
Scenario Outline: Store and read a value
	Given Treesor is running at localhost and 9002
	Given I store <value> at hierarchy position <path>
	When I read <path>
	Then the response contains <path> and <value>
Examples: 
	| path		| value |
	| a/b		| test  |
	| root-path | test2 |
