# Milestone 4.7 Test Run Summary

* **Total Tests:** 136
* **Passed:** 85
* **Failed:** 51
* **Outcome:** Failed

---

## [X] Failed Test Details (51)
### FAIL: DemoERPApi.Tests.Integration.Authorization.CustomerRoleTests.AUTH_014_CustomerRetrievesAnotherCustomer_ReturnsForbidden
>>> **Error:** System.Exception : Seed sync data execution rejected: Unauthorized. Details:

### FAIL: DemoERPApi.Tests.Integration.Authorization.AnonymousRoleTests.AUTH_040e_UpdateCustomerAlternative_WithAnonymousRoleClaimToken_ReturnsForbidden
>>> **Error:** System.Exception : Seed sync data execution rejected: Unauthorized. Details:

### FAIL: DemoERPApi.Tests.Integration.Authorization.AdminRoleTests.AUTH_011c_AdminDeletesAnotherCustomerAccount_ReturnsOk
>>> **Error:** System.Exception : Seed sync data execution rejected: Unauthorized. Details:

### FAIL: DemoERPApi.Tests.Integration.Validation.CustomerValidationTests.VALID_009_CreateMalformedJsonPayload_ReturnsBadRequest
>>> **Error:** Assert.Equal() Failure: Values differ
Expected: BadRequest
Actual:   NotFound

### FAIL: DemoERPApi.Tests.Integration.Authorization.QaRoleTests.AUTH_012_GetCustomer_AssignedToDifferentQA_ReturnsForbidden
>>> **Error:** System.Exception : Seed sync data execution rejected: Unauthorized. Details:

### FAIL: DemoERPApi.Tests.Integration.Authorization.AdminRoleTests.AUTH_011b_AdminUpdatesAnotherCustomerProfile_ReturnsOk
>>> **Error:** System.Exception : Seed sync data execution rejected: Unauthorized. Details:

### FAIL: DemoERPApi.Tests.Integration.Authorization.AdminRoleTests.AUTH_011e_AdminRetrievesOwnCustomerRecord_ReturnsOk
>>> **Error:** System.Exception : Seed sync data execution rejected: Unauthorized. Details:

### FAIL: DemoERPApi.Tests.Integration.Customer.CustomerUpdateTests.UPDATE_019_UpdateDeletedCustomer_ReturnsNotFoundOrBadRequest
>>> **Error:** System.Exception : Seed sync data execution rejected: Unauthorized. Details:

### FAIL: DemoERPApi.Tests.Integration.Validation.CustomerValidationTests.VALID_011_CreateInvalidEmail_ReturnsBadRequest
>>> **Error:** Assert.Equal() Failure: Values differ
Expected: BadRequest
Actual:   NotFound

### FAIL: DemoERPApi.Tests.Integration.Authorization.CustomerRoleTests.AUTH_032_CustomerDeletesAnotherCustomerAccount_ReturnsForbidden
>>> **Error:** System.Exception : Seed sync data execution rejected: Unauthorized. Details:

### FAIL: DemoERPApi.Tests.Integration.Authorization.CustomerRoleTests.AUTH_050_CustomerAttemptsToCreateCustomer_ReturnsForbidden
>>> **Error:** Assert.Equal() Failure: Values differ
Expected: Forbidden
Actual:   OK

### FAIL: DemoERPApi.Tests.Integration.Customer.CustomerSyncTests.SYNC_003_QACreatesAssignedCustomer_IfPermitted_ReturnsOk
>>> **Error:** Assert.Equal() Failure: Values differ
Expected: OK
Actual:   Forbidden

### FAIL: DemoERPApi.Tests.Integration.Customer.CustomerDeleteTests.DELETE_007_CustomerDeletesOwnAccountIfPermitted_ReturnsForbidden
>>> **Error:** System.Exception : Seed sync data execution rejected: Unauthorized. Details:

### FAIL: DemoERPApi.Tests.Integration.Security.SecurityTests.SEC_014_GetCustomer_CustomerRetrievesOwnCustomerRecord_ReturnsOk
>>> **Error:** System.Exception : Seed sync data execution rejected: Unauthorized. Details:

### FAIL: DemoERPApi.Tests.Integration.Customer.CustomerDeleteTests.DELETE_009_DeletedCustomer_ExcludedFromNormalReads
>>> **Error:** System.Exception : Seed sync data execution rejected: Unauthorized. Details:

### FAIL: DemoERPApi.Tests.Integration.Security.SecurityTests.SEC_012_CustomerApis_SqlInjectionOrXssPayload_ReturnsBadRequest
>>> **Error:** Assert.Equal() Failure: Values differ
Expected: BadRequest
Actual:   OK

### FAIL: DemoERPApi.Tests.Integration.Customer.CustomerUpdateTests.UPDATE_001_UpdatePayloadDuplicatesAnotherCustomer_ReturnsConflict
>>> **Error:** System.Exception : Seed sync data execution rejected: Unauthorized. Details:

### FAIL: DemoERPApi.Tests.Integration.Customer.CustomerDeleteTests.DELETE_001_AdminDeletesExistingCustomer_ReturnsOk
>>> **Error:** System.Exception : Seed sync data execution rejected: Unauthorized. Details:

### FAIL: DemoERPApi.Tests.Integration.Customer.CustomerUpdateTests.UPDATE_006_QAUpdatesAssignedCustomer_WithValidPayload_ReturnsOk
>>> **Error:** System.Exception : Seed sync data execution rejected: Unauthorized. Details:

### FAIL: DemoERPApi.Tests.Integration.Authorization.QaRoleTests.AUTH_028_UpdateCustomer_OutsideAssignmentScope_ReturnsForbidden
>>> **Error:** System.Exception : Seed sync data execution rejected: Unauthorized. Details:

### FAIL: DemoERPApi.Tests.Integration.Authorization.AnonymousRoleTests.AUTH_040d_DeleteCustomer_WithAnonymousRoleClaimToken_ReturnsForbidden
>>> **Error:** System.Exception : Seed sync data execution rejected: Unauthorized. Details:

### FAIL: DemoERPApi.Tests.Integration.Validation.CustomerValidationTests.VALID_010_CreateMissingRequiredFields_ReturnsBadRequest
>>> **Error:** Assert.Equal() Failure: Values differ
Expected: BadRequest
Actual:   NotFound

### FAIL: DemoERPApi.Tests.Integration.Authorization.CustomerRoleTests.AUTH_029_CustomerUpdatesAnotherCustomerProfile_ReturnsForbidden
>>> **Error:** System.Exception : Seed sync data execution rejected: Unauthorized. Details:

### FAIL: DemoERPApi.Tests.Integration.Customer.CustomerGetTests.GET_017_RequestDeletedCustomerRecord_ReturnsNotFound
>>> **Error:** System.Exception : Seed sync data execution rejected: Unauthorized. Details:

### FAIL: DemoERPApi.Tests.Integration.Authentication.AuthControllerTests.AUTH_008_EmptyJsonBody_ReturnsBadRequest
>>> **Error:** Assert.Equal() Failure: Values differ
Expected: BadRequest
Actual:   Unauthorized

### FAIL: DemoERPApi.Tests.Integration.Authorization.UnsupportedRoleTests.AUTH_058_UpdateCustomer_UnsupportedRoleValue_ReturnsForbidden
>>> **Error:** Assert.Equal() Failure: Values differ
Expected: Forbidden
Actual:   NotFound

### FAIL: DemoERPApi.Tests.Integration.Customer.CustomerSyncTests.SYNC_015_SyncDuplicateAgainstSoftDeletedCustomer_HandlesCorrectly
>>> **Error:** Assert.True() Failure
Expected: True
Actual:   False

### FAIL: DemoERPApi.Tests.Integration.Customer.CustomerGetTests.GET_001_AdminRetrievesExistingCustomer_ReturnsOk
>>> **Error:** System.Exception : Seed sync data execution rejected: Unauthorized. Details:

### FAIL: DemoERPApi.Tests.Integration.Authorization.QaRoleTests.AUTH_027_UpdateCustomer_AssignedToDifferentQA_ReturnsForbidden
>>> **Error:** System.Exception : Seed sync data execution rejected: Unauthorized. Details:

### FAIL: DemoERPApi.Tests.Integration.Authorization.QaRoleTests.AUTH_013_GetCustomer_OutsideAssignmentScope_ReturnsForbidden
>>> **Error:** System.Exception : Seed sync data execution rejected: Unauthorized. Details:

### FAIL: DemoERPApi.Tests.Integration.Security.SecurityTests.SEC_015_UpdateCustomer_CustomerUpdatesOwnProfile_ReturnsOk
>>> **Error:** System.Exception : Seed sync data execution rejected: Unauthorized. Details:

### FAIL: DemoERPApi.Tests.Integration.Authorization.QaRoleTests.AUTH_030_DeleteCustomer_AssignedToDifferentQA_ReturnsForbidden
>>> **Error:** System.Exception : Seed sync data execution rejected: Unauthorized. Details:

### FAIL: DemoERPApi.Tests.Integration.Customer.CustomerDeleteTests.DELETE_010_DeleteAlreadyDeletedCustomer_ReturnsNotFound
>>> **Error:** System.Exception : Seed sync data execution rejected: Unauthorized. Details:

### FAIL: DemoERPApi.Tests.Integration.Authentication.AuthControllerTests.AUTH_003_ValidLoginCustomer_ReturnsOk
>>> **Error:** Assert.Equal() Failure: Values differ
Expected: OK
Actual:   Unauthorized

### FAIL: DemoERPApi.Tests.Integration.Authorization.UnsupportedRoleTests.AUTH_054_GetCustomer_UnsupportedRoleValue_ReturnsForbidden
>>> **Error:** Assert.Equal() Failure: Values differ
Expected: Forbidden
Actual:   NotFound

### FAIL: DemoERPApi.Tests.Integration.Authorization.AnonymousRoleTests.AUTH_040c_UpdateCustomer_WithAnonymousRoleClaimToken_ReturnsForbidden
>>> **Error:** System.Exception : Seed sync data execution rejected: Unauthorized. Details:

### FAIL: DemoERPApi.Tests.Integration.Validation.CustomerValidationTests.VALID_012_CreateInvalidPhone_ReturnsBadRequest
>>> **Error:** Assert.Equal() Failure: Values differ
Expected: BadRequest
Actual:   NotFound

### FAIL: DemoERPApi.Tests.Integration.Authorization.AdminRoleTests.AUTH_011a_AdminRetrievesCustomerOwnedByAnotherRole_ReturnsOk
>>> **Error:** System.Exception : Seed sync data execution rejected: Unauthorized. Details:

### FAIL: DemoERPApi.Tests.Integration.Customer.CustomerDeleteTests.DELETE_004_QADeletesAssignedCustomer_ReturnsOk
>>> **Error:** System.Exception : Seed sync data execution rejected: Unauthorized. Details:

### FAIL: DemoERPApi.Tests.Integration.Customer.CustomerDeleteTests.DELETE_008_SoftDeleteSetsIsDeleted_InDatabase
>>> **Error:** System.Exception : Seed sync data execution rejected: Unauthorized. Details:

### FAIL: DemoERPApi.Tests.Integration.Customer.CustomerUpdateTests.UPDATE_004_AdminUpdatesAnyCustomer_WithValidPayload_ReturnsOk
>>> **Error:** System.Exception : Seed sync data execution rejected: Unauthorized. Details:

### FAIL: DemoERPApi.Tests.Integration.Authorization.AnonymousRoleTests.AUTH_040f_DeleteCustomerAlternative_WithAnonymousRoleClaimToken_ReturnsForbidden
>>> **Error:** System.Exception : Seed sync data execution rejected: Unauthorized. Details:

### FAIL: DemoERPApi.Tests.Integration.Authorization.AdminRoleTests.AUTH_011g_AdminDeletesOwnCustomerAccount_ReturnsOk
>>> **Error:** System.Exception : Seed sync data execution rejected: Unauthorized. Details:

### FAIL: DemoERPApi.Tests.Integration.Customer.CustomerDeleteTests.DELETE_006_QADeletesNonExistentAssignedCustomer_ReturnsNotFound
>>> **Error:** Assert.Equal() Failure: Values differ
Expected: NotFound
Actual:   Forbidden

### FAIL: DemoERPApi.Tests.Integration.Authorization.AnonymousRoleTests.AUTH_040b_GetCustomer_WithAnonymousRoleClaimToken_ReturnsForbidden
>>> **Error:** System.Exception : Seed sync data execution rejected: Unauthorized. Details:

### FAIL: DemoERPApi.Tests.Integration.Customer.CustomerGetTests.GET_004_QARetrievesAssignedCustomer_ReturnsOk
>>> **Error:** System.Exception : Seed sync data execution rejected: Unauthorized. Details:

### FAIL: DemoERPApi.Tests.Integration.Authorization.AdminRoleTests.AUTH_011f_AdminUpdatesOwnCustomerProfile_ReturnsOk
>>> **Error:** System.Exception : Seed sync data execution rejected: Unauthorized. Details:

### FAIL: DemoERPApi.Tests.Integration.Authorization.UnsupportedRoleTests.AUTH_057_UpdateCustomer_AnonymousRoleValue_ReturnsForbidden
>>> **Error:** Assert.Equal() Failure: Values differ
Expected: Forbidden
Actual:   NotFound

### FAIL: DemoERPApi.Tests.Integration.Authentication.AuthControllerTests.AUTH_002_ValidLoginQA_ReturnsOk
>>> **Error:** Assert.Equal() Failure: Values differ
Expected: OK
Actual:   Unauthorized

### FAIL: DemoERPApi.Tests.Integration.Authorization.UnsupportedRoleTests.AUTH_053_GetCustomer_AnonymousRoleValue_ReturnsForbidden
>>> **Error:** Assert.Equal() Failure: Values differ
Expected: Forbidden
Actual:   NotFound

### FAIL: DemoERPApi.Tests.Integration.Authorization.QaRoleTests.AUTH_031_DeleteCustomer_OutsideAssignmentScope_ReturnsForbidden
>>> **Error:** System.Exception : Seed sync data execution rejected: Unauthorized. Details:


