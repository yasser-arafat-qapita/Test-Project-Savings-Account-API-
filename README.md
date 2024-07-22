# Test Project : Savings-Account-API
### Project follows segregation of Write and Read i.e CQRS pattern and Domain Driven Design ( DDD )
### This is a Test Project that includes implementations of following stack:
1. EventStore DB
2. Mongo DB & Projection to MongoDB
3. Creation/Processing of Hangfire Jobs


### API Routes :
1. Create Savings Account with unique account_id and opening balance.
2. Update Savings Account with account_id, transaction_type ("Credit", "Debit" ) and transaction_amount.
3. Generate Account Statement using account_id.


### Account Statement Excel saved at user's desktop:
Sample

![excel](https://github.com/user-attachments/assets/73769c3a-cc8c-4c8c-a490-f0ba5896a772)


### MongoDB Projected Data ("TestBank"):
MongoDB stores only the latest account balance.

![image](https://github.com/user-attachments/assets/533d0eb9-4f6d-4928-9b87-b174b1d7b3cd)


### EventStore : 
Each Account in the Savings Account is an EventStore Stream using the unique account_id.
Each EventStore stream stores the account's different transactions.

![image](https://github.com/user-attachments/assets/5824ceca-4bfc-41ea-ad5d-6d157cf36689)

## Project's WEB API Flow:
![image](https://github.com/user-attachments/assets/34689777-5dda-4cf4-aa51-56e83b18f1d0)

