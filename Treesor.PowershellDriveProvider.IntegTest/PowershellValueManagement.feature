Feature: PowershellValueManagement
	As a powershell client
	I want to create, modify delete vaues at root node

@powershell
Scenario Outline: Create a new value
	Given Treesor is running at localhost and 9002
	Given TreesorDriveProvider is imported
	When I set <value> at hierarchy position <path>
	Then the result should be 120 on the screen

Examples: 
	| path		| value |
	| treesor:/ | test2 |

