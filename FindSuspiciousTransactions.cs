using System;
using System.Collections.Generic;
using System.Linq;

/*
    Part 1 - Very large transactions
    First we will detect transactions that are very large. Your function will be passed the data in data/part1.json, and it should return a list of IDs for transactions with value at least $10,000 (list ordering does not matter).

    Part 2 - Large initial transactions
    For the previous phase we considered every transaction individually. Now we will consider multiple transactions at the same time.

    Your task is to identify when a transaction with value at least $1,000 occurs between two people who have not previously been involved in a transaction together (in either direction). Your function will be passed the data in data/part2.json, and it should return a list of IDs for transactions that:

    have value at least $1,000
    are between two people who have not previously had a transaction together, in either direction, of any value
    Note: Only the first such transaction between such pair of people should be marked as suspicious. Subsequent large transactions between the same pair are fine, even if the first one was suspicious.

    Part 3 - Exploration
    Now it is your job to design the next feature(s) of fraud risk assessment. Look at the sample data in data/part3.json. Can you think of any additional heuristics that could be used to identify suspicious transactions in this dataset? Your function should return a list of IDs for transactions matching your new heuristic(s).

    There is no right or wrong answer for Part 3. It is OK if you classify some legitimate transactions as suspicious: no fraud detection is 100% reliable. However, a Plaid engineer will review your code, so please make sure to include comments explaining the design and/or implementation of your heuristics.

    Don't worry about designing a world-class fraud detection system. One or two simple heuristics are fine, as long as you explain your thinking.
*/

namespace FindSuspiciousTransactions
{
    public class Geotag
    {
        public string CountryCode { get; set; }
        public float Lat { get; set; }
        public float Lon { get; set; }
    }

    public class User
    {
        public string UserId { get; set; }
        public string Name { get; set; }
        public Geotag Home { get; set; }
    }

    public class Party
    {
        public string UserId { get; set; }
        public Geotag Geotag { get; set; }
    }

    public class Transaction
    {
        public string TransactionId { get; set; }
        public Party Payer { get; set; }
        public Party Payee { get; set; }
        public DateTime Timestamp { get; set; }
        public double Amount { get; set; }
        public string Category { get; set; }
    }

    public class Data
    {
        public List<User> Users { get; set; }
        public List<Transaction> Transactions { get; set; }
    }

    public class FindSuspiciousTransactions
    {
        private const double VeryLargeTransactionsMinValue = 10000;
        private const double LargeTransactionsMinValue = 1000;
        
        public static List<string> Part1_FindLargeTransactions(Data data) {
            // filtering all the IDs for transactions > 1000 
            
            return data.Transactions
            .Where(t => t.Amount >= VeryLargeTransactionsMinValue)
            .Select(t => t.TransactionId)
            .ToList();
        }

        public static List<string> Part2_FindLargeInitialTransactions(Data data)
        {
            var largeInitialTransactions = new List<string>();
            var history = new Dictionary<string, HashSet<string>>();
            
            // there's no information if the transaction file is ordered by date
            // so first off all, make sure our transactions are ordered
            
            foreach (var t in data.Transactions.OrderBy(t => t.Timestamp)) {
                // we keep our transactions in a hashmap cointaining the history
                // of operations between two users, and we check the history 
                // and amount to see if this transaction is large and initial
                
                if (t.Amount >= LargeTransactionsMinValue 
                && !InvolvedInPreviousTransaction(history, t.Payer.UserId, t.Payee.UserId)) 
                {
                    largeInitialTransactions.Add(t.TransactionId);
                }
                
                // save all the transactions in the history
                if (!history.ContainsKey(t.Payer.UserId)) {
                    history[t.Payer.UserId] = new HashSet<string>();
                }
                history[t.Payer.UserId].Add(t.Payee.UserId);
            }
            
            return largeInitialTransactions;
        }

        public static List<string> Part3_FindSuspiciousTransactions(Data data)
        {
            /* I will consider a suspicious transactions
               1. Trasactions with same payer, same payee, same value 
               in a interval shorter than 10 minutes
               
               2. Transactions with same payer, same payee, but in different                    countries */
                  
            /* First we group the transactions by payer and payee that's
               our common criteria. Timestamp is important for criteria no 1.
               We can already eliminate groups with only one transaction.
            */
            var transactionGroups = data.Transactions.OrderBy(t => t.Timestamp)
                .GroupBy(t => new {
                    PayerId = t.Payer.UserId,
                    PayeeId = t.Payee.UserId
                })
                .Where(tg => tg.Count() > 1)
                .Select(ts => new {
                    Transactions = ts.ToList()
                });   
            
            var suspiciousTransactions = new List<string>();      
            foreach (var tg in transactionGroups) 
            {
                // a transaction group is suspicious if one of them occurs
                // in a different country
                var suspicious = tg.Transactions.GroupBy(t => new {
                        PayerCountryCode = t.Payer.Geotag.CountryCode,
                        PayeeCountryCode = t.Payee.Geotag.CountryCode
                    }).Count() > 1;
                
                
                if (!suspicious) {
                    Transaction previousTransaction = null;
                    foreach (var t in tg.Transactions) {
                        // check if the amount is the same and time difference is short
                        if (previousTransaction != null
                            && previousTransaction.Amount == t.Amount
                            && (t.Timestamp - previousTransaction.Timestamp).Minutes < 10) 
                        {
                            suspicious = true;
                        }
                        
                        previousTransaction = t;
                    }
                }
                
                if (suspicious) {
                     suspiciousTransactions.AddRange(tg.Transactions.Select(t => t.TransactionId));
                }
            }
            
            return suspiciousTransactions;
        }
        
        private static bool InvolvedInPreviousTransaction(Dictionary<string, HashSet<string>> history, string payerId, string payeeId) {
            // make sure that the transaction could have happened both ways
            if ((history.ContainsKey(payerId) && history[payerId].Contains(payeeId))
            || (history.ContainsKey(payeeId) && history[payeeId].Contains(payerId))) {
                return true;
            }
            
            return false;
        }
    }
}
