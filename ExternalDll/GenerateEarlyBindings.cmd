REM
REM Use CRMSVCUtil to regenerate the Early Bindings.
..\coretools\crmsvcutil.exe /url:https://rbh3.api.crm11.dynamics.com/XRMServices/2011/Organization.svc /out:Entities.cs ^
   /username:roger@rbh3.onmicrosoft.com /password:Password123 ^
   /namespace:DynamicsLinq /serviceContextName:OrgServiceContext
	