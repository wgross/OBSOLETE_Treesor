Feature: ValueManagement
	As a web client
	I want to store, read and remove values with a hierarchical key

@cmd
Scenario Outline: Create and read a value
	Given Treesor is running at localhost and 9002
	And I create <value> at hierarchy position <path>
	When I read <path>
	Then Read response is 200
	Then Read response contains <path> and <value>
Examples: 
	| path		| value |
	| a/b		| test  |
	| root-path | test2 |

@cmd
Scenario Outline: Create and delete a value
	Given Treesor is running at localhost and 9002
	And I create <value> at hierarchy position <path>
	When I delete at hierarchy position <path>
	Then Delete response is 200
	When I read <path>
	Then Read response is 404
Examples: 
	| path		| value |
	| a/b		| test  |
	| root-path | test2 |

@cmd
Scenario Outline: Create and update a value
	Given Treesor is running at localhost and 9002
	And I create <value> at hierarchy position <path>
	When I update with <newValue> at hierarchy position <path>
	Then update response is 200
	And update result contains <path> and <newValue>
Examples: 
	| path      | value | newValue |
	| a/b       | test  | test3    |
	| root-path | test2 | test4    |

	

