# FluentCRM
This project provieds a Fluent-style interface to Micorosft Dynamics CRM using the Microsoft.Xrm.Sdk web interface.

This project arose from the frustrations of working with the existing SDK interface and dealing with various issues: -

1) Using late bound - Having to specificy in multiple places the name of the attribute to load from the server, and then the name to operate on
   This means we have the same information in multiple places, leading to mismatches and errors.
   
2) The issues around "phantom" updates to CRM attributes, where poorly written client updates an attribute to the same value and writes it back 
   to the server. This raises issues with clutter in the audit log, but more importantly can lead to workflows and plugins running even when there
   has been no change to an attribute.

3) Writing for the CRM SDK interface produces very "noisy" code. Another aim was to take advantage generics and a Fluent-style interface to produce code that is easy to write and easy to read.

4) Thy to avoid the "Retreive of death" where a developer gives up trying to track which attributes they need and just pulls the whole entity back - just in case they have forgotten to include a field they need.

5) Any interface has to be easily extended to cover the multiple different entity types found in a CRM installation.

6) Have you ever been asaked to return a phone number from CRM, and the requirement says use the work number, or if that is missing use the mobile number. Oh and lets throw in the home number just in case the first two are missing? I had that problem, so FluentCRM can help there too.

A taste of Fluent: -

```C#
 FluentContact.Contact(contact.Id,  service)
               .WeakExtract( (string e) => myStruct.emailAddress = e, "emailaddress1")
               .Execute();
```

This fetches a contact by its ID, and if found it will call the closure to set myStruct.emailAddress to the value.
Note that you have to only specify the attribute name once. Only this attribute will be retreived from CRM.
Also observe that nothing happens before the final "Execute()".

```C#
 FluentContact.Contact(contact.Id,  service)
                .WeakExtract( (string e) => myStruct.emailAddress = e, "emailaddress1")
                .WeakExtract<string>( num =>  myStruct.phoneNumber =  num, "mobilephone", "telephone1" )
                .Execute();
```

This adds a clause to extract a phone number from either the "mobilephone" or "telephone1" fields (checking in the specified order for a field that is not empty and then calling the closure only once with the found value. If no value is found then the closure will not be called)

```C#
FluentContact.Contact(contact.Id,  service)
                .WeakExtract( (string e) => myStruct.emailAddress = e, "emailaddress1")
                .WeakExtract<string>( num =>  myStruct.phoneNumber =  num, "mobilephone", "telephone1" )
                .WeakExtractEntity(
                    con => myStruct.name = myStruct.contactName =
                        string.Join(" ", con.Attributes.firstname, con.Attributes.lastname),
                    "firstname", "lastname")
                .Execute();
```

Adds a WeakExtractEntity clause. This will call the closue with an "entity" fetched with the listed attributes.
