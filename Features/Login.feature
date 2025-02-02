Feature: Login
  As a user
  I want to log in to the application

  Scenario: Successful Login
    Given I enter valid credentials and Perform Login
    Then I should be redirected to the dashboard