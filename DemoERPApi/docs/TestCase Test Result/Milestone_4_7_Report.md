# Milestone 4.7 Test Run Summary

* **Total Tests:** 136
* **Passed:** 104
* **Failed:** 32
* **Outcome:** Failed

---

## [X] Failed Test Details (32)
### FAIL: DemoERPApi.Tests.Integration.Customer.CustomerSyncTests.SYNC_015_SyncDuplicateAgainstSoftDeletedCustomer_HandlesCorrectly
>>> **Error:** Assert.True() Failure
Expected: True
Actual:   False

### FAIL: DemoERPApi.Tests.Integration.Validation.CustomerValidationTests.VALID_011_CreateInvalidEmail_ReturnsBadRequest
>>> **Error:** Assert.Equal() Failure: Values differ
Expected: BadRequest
Actual:   NotFound

### FAIL: DemoERPApi.Tests.Integration.Authentication.AuthControllerTests.AUTH_002_ValidLoginQA_ReturnsOk
>>> **Error:** Assert.Equal() Failure: Values differ
Expected: OK
Actual:   Unauthorized

### FAIL: DemoERPApi.Tests.Integration.Authorization.CustomerRoleTests.AUTH_032_CustomerDeletesAnotherCustomerAccount_ReturnsForbidden
>>> **Error:** System.Exception : Seed failed:

### FAIL: DemoERPApi.Tests.Integration.Authentication.AuthControllerTests.AUTH_003_ValidLoginCustomer_ReturnsOk
>>> **Error:** Assert.Equal() Failure: Values differ
Expected: OK
Actual:   Unauthorized

### FAIL: DemoERPApi.Tests.Integration.Authorization.UnsupportedRoleTests.AUTH_054_GetCustomer_UnsupportedRoleValue_ReturnsForbidden
>>> **Error:** Assert.Equal() Failure: Values differ
Expected: Forbidden
Actual:   NotFound

### FAIL: DemoERPApi.Tests.Integration.Security.SecurityTests.SEC_015_UpdateCustomer_CustomerUpdatesOwnProfile_ReturnsOk
>>> **Error:** Assert.Equal() Failure: Values differ
Expected: OK
Actual:   Forbidden

### FAIL: DemoERPApi.Tests.Integration.Authorization.UnsupportedRoleTests.AUTH_053_GetCustomer_AnonymousRoleValue_ReturnsForbidden
>>> **Error:** Assert.Equal() Failure: Values differ
Expected: Forbidden
Actual:   NotFound

### FAIL: DemoERPApi.Tests.Integration.Authorization.CustomerRoleTests.AUTH_029_CustomerUpdatesAnotherCustomerProfile_ReturnsForbidden
>>> **Error:** System.Exception : Seed failed:

### FAIL: DemoERPApi.Tests.Integration.Security.SecurityTests.SEC_012_CustomerApis_SqlInjectionOrXssPayload_ReturnsBadRequest
>>> **Error:** Assert.Equal() Failure: Values differ
Expected: BadRequest
Actual:   OK

### FAIL: DemoERPApi.Tests.Integration.Validation.CustomerValidationTests.VALID_012_CreateInvalidPhone_ReturnsBadRequest
>>> **Error:** Assert.Equal() Failure: Values differ
Expected: BadRequest
Actual:   NotFound

### FAIL: DemoERPApi.Tests.Integration.Authorization.QaRoleTests.AUTH_013_GetCustomer_OutsideAssignmentScope_ReturnsForbidden
>>> **Error:** System.Exception : Seed failed:

### FAIL: DemoERPApi.Tests.Integration.Authorization.QaRoleTests.AUTH_030_DeleteCustomer_AssignedToDifferentQA_ReturnsForbidden
>>> **Error:** System.Exception : Seed failed:

### FAIL: DemoERPApi.Tests.Integration.Authorization.QaRoleTests.AUTH_031_DeleteCustomer_OutsideAssignmentScope_ReturnsForbidden
>>> **Error:** System.Exception : Seed failed:

### FAIL: DemoERPApi.Tests.Integration.Authentication.AuthControllerTests.AUTH_008_EmptyJsonBody_ReturnsBadRequest
>>> **Error:** Assert.Equal() Failure: Values differ
Expected: BadRequest
Actual:   Unauthorized

### FAIL: DemoERPApi.Tests.Integration.Customer.CustomerDeleteTests.DELETE_006_QADeletesNonExistentAssignedCustomer_ReturnsNotFound
>>> **Error:** Assert.Equal() Failure: Values differ
Expected: NotFound
Actual:   Forbidden

### FAIL: DemoERPApi.Tests.Integration.Authorization.UnsupportedRoleTests.AUTH_057_UpdateCustomer_AnonymousRoleValue_ReturnsForbidden
>>> **Error:** Assert.Equal() Failure: Values differ
Expected: Forbidden
Actual:   NotFound

### FAIL: DemoERPApi.Tests.Integration.Customer.CustomerUpdateTests.UPDATE_001_UpdatePayloadDuplicatesAnotherCustomer_ReturnsConflict
>>> **Error:** Assert.Equal() Failure: Values differ
Expected: Conflict
Actual:   OK

### FAIL: DemoERPApi.Tests.Integration.Authorization.QaRoleTests.AUTH_012_GetCustomer_AssignedToDifferentQA_ReturnsForbidden
>>> **Error:** System.Exception : Seed failed:

### FAIL: DemoERPApi.Tests.Integration.Customer.CustomerDeleteTests.DELETE_004_QADeletesAssignedCustomer_ReturnsOk
>>> **Error:** System.Exception : Seed failed:

### FAIL: DemoERPApi.Tests.Integration.Validation.CustomerValidationTests.VALID_009_CreateMalformedJsonPayload_ReturnsBadRequest
>>> **Error:** Assert.Equal() Failure: Values differ
Expected: BadRequest
Actual:   NotFound

### FAIL: DemoERPApi.Tests.Integration.Authorization.QaRoleTests.AUTH_027_UpdateCustomer_AssignedToDifferentQA_ReturnsForbidden
>>> **Error:** System.Exception : Seed failed:

### FAIL: DemoERPApi.Tests.Integration.Authorization.CustomerRoleTests.AUTH_014_CustomerRetrievesAnotherCustomer_ReturnsForbidden
>>> **Error:** System.Exception : Seed failed:

### FAIL: DemoERPApi.Tests.Integration.Authorization.QaRoleTests.AUTH_028_UpdateCustomer_OutsideAssignmentScope_ReturnsForbidden
>>> **Error:** System.Exception : Seed failed:

### FAIL: DemoERPApi.Tests.Integration.Customer.CustomerSyncTests.SYNC_003_QACreatesAssignedCustomer_IfPermitted_ReturnsOk
>>> **Error:** Assert.Equal() Failure: Values differ
Expected: OK
Actual:   Forbidden

### FAIL: DemoERPApi.Tests.Integration.Authorization.CustomerRoleTests.AUTH_050_CustomerAttemptsToCreateCustomer_ReturnsForbidden
>>> **Error:** Assert.Equal() Failure: Values differ
Expected: Forbidden
Actual:   OK

### FAIL: DemoERPApi.Tests.Integration.Authorization.UnsupportedRoleTests.AUTH_058_UpdateCustomer_UnsupportedRoleValue_ReturnsForbidden
>>> **Error:** Assert.Equal() Failure: Values differ
Expected: Forbidden
Actual:   NotFound

### FAIL: DemoERPApi.Tests.Integration.Customer.CustomerDeleteTests.DELETE_007_CustomerDeletesOwnAccountIfPermitted_ReturnsForbidden
>>> **Error:** Assert.Equal() Failure: Values differ
Expected: Forbidden
Actual:   OK

### FAIL: DemoERPApi.Tests.Integration.Customer.CustomerUpdateTests.UPDATE_006_QAUpdatesAssignedCustomer_WithValidPayload_ReturnsOk
>>> **Error:** System.Exception : Seed failed:

### FAIL: DemoERPApi.Tests.Integration.Customer.CustomerGetTests.GET_004_QARetrievesAssignedCustomer_ReturnsOk
>>> **Error:** System.Exception : Seed failed:

### FAIL: DemoERPApi.Tests.Integration.Validation.CustomerValidationTests.VALID_010_CreateMissingRequiredFields_ReturnsBadRequest
>>> **Error:** Assert.Equal() Failure: Values differ
Expected: BadRequest
Actual:   NotFound

### FAIL: DemoERPApi.Tests.Integration.Security.SecurityTests.SEC_014_GetCustomer_CustomerRetrievesOwnCustomerRecord_ReturnsOk
>>> **Error:** Assert.Equal() Failure: Values differ
Expected: OK
Actual:   Forbidden


