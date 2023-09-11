# Support Bank

You're working for a support team who like to run social events for each other. They mostly operate on an IOU basis and keep records of who owes money to whom. Over time though, these records have gotten a bit out of hand. Your job is to write a program which reads their records and works out how much money each member of the support team owes.

Each IOU can be thought of as one person paying another, but you can perform this via an intermediary, like moving money between bank accounts. Instead of Alice paying Bob directly, Alice pays the money to a central bank, and Bob receives money from the central bank.

## The specification

Your program should support four commands, which can be typed in the console while the program is running:

- `load <filepath>` - should load transactions from the file located at `<filepath>` into the bank, overwriting any transactions that were there before. You should implement the `Bank` class's `LoadTransactionsFromFile` method to achieve this.
  > ### Specific requirements
  > - You must use the pre-existing `streamReader` variable at the top of the method to access the contents of the file located at `<filepath>`.
  > - If there is no file located at `<filepath>` or if the file fails to load for another reason, you must write the error message located in the `Bank` class's `UnableToLoadFile` variable to the console.
  > - If the file located at `<filepath>` exists but has an unsupported file extension, you must write the error message located in the `Bank` class's `UnsupportedFileExtension` variable to the console.
  > - If neither of these errors occurs, you must clear the existing `Transactions` list in the `Bank` class, populate it with transactions from the file located at `<filepath>`, and then write the message located in the `Bank` class's `LoadingComplete` variable to the console.
- `list all` - should output the name of each person and the total amount of money they should receive from the bank. This could be a negative number (i.e. if they owe money to the bank). You should implement the `Bank` class's `ListAllAccountsAndBalances` method to achieve this.
  > ### Specific requirements
  > - If the `Transactions` list in the `Bank` class is empty, you must write the message located in the `Account` class's `NonePresent` variable to the console.
  > - Otherwise, you must write the `Account` class's `HeaderRow` and `HeaderRule` variables to the console, populate a list of accounts based on the transactions currently in the `Bank` class's `Transactions` list, and finally write each account to the console.
- `list <name>` - should print a list of every transaction that the named person was involved with. For example, `list Jon A` would list all of Jon A's transactions. You should implement the `Bank` class's `ListAllTransactionsForAccount` method to achieve this.
  > ### Specific requirements
  > - If the `Transactions` list in the `Bank` class is empty, you must write the message located in the `Transaction` class's `NonePresent` variable to the console.
  > - Otherwise, you must write the `Transaction` class's `HeaderRow` and `HeaderRule` variables, as well as each transaction from the `Bank` class's `Transactions` list that involves the named person, to the console.
- `exit` - should exit the program (already implemented).

Any other commands should result in an output of `"Your command was not understood. Please try again."` (already implemented).

## Reading CSV files

The support team keep their records in CSV format. Their records for 2014 are stored in Transactions2014.csv. Note that there's a header row telling you what each column means. Every record has a date and represents a payment from one person to another person. There's also a narrative field which describes what the payment was for.

Your program should be able to load files with a .csv extension.

## Logging and exception handling

Next, we'll be modifying the program so that it also loads all of the records for 2015 from Transactions2015.csv.

You'll probably notice that some dodgy data is present in the file and your program fails in some interesting ways.

Firstly, add some logging to your program. A logger has already been configured inside the `Bank` class in the form of the `_logger` variable, with an example log statement present at the top of the `ServeCustomer` method.

> ### Specific requirements
> Your program should contain the following log statements:
> - Any information-level statements you feel would be useful, such as when any command is run
> - A warning-level statement when the `list all` command is run with an empty `Transactions` list
> - A warning-level statement when the `list <name>` command is run with an empty `Transactions` list
> - An error-level statement when the `load <filepath>` command is run with a non-existent or otherwise non-loadable file
> - An error-level statement when the `load <filepath>` command is run with a file with an unsupported file extension
> - An error-level statement whenever a row from the CSV file currently being loaded is formatted incorrectly

You should now have forensic evidence to work out why things went wrong. Now change your program so that it fails gracefully and tells the user which row(s) of the CSV caused any problems.

> Think about what failing gracefully means in this situation. Should we import the remaining transactions from the file? Should we just stop at the line that failed? Could we validate the file before loading and tell the user where all of the errors are? What would make most sense if you were using the software?

For the purpose of this exercise, we will load all but the badly-formatted records into the `Bank` class's `Transactions` list, and write a message beginning with `"Unable to parse transaction"` to the console for each badly-formatted transaction (in addition to the error-level log statement added previously). We should probably also write some identifying information so that the user knows which record(s) caused any failures.

## Reading JSON files

Back in 2013, the support team didn't store their records in CSV format. They stored them in a different format, called JSON. Take a look at the 2013 records file, Transactions2013.json. Hopefully it's fairly obvious how how the transactions in JSON format correspond to the newer CSV transactions.

Modify your program so that it can load files with a .json extension as well as those with a .csv extension.

## Reading XML files

The support team's transactions for 2012 were in XML, another commonly-used data format. Modify your program so that it can load files with a .xml extension as well as those with a .csv or .json extension.

Now that you've reached the end, look over your code again. Think about the classes in your program and the relationships between them. Try to keep your classes and methods focused, make their functionality obvious based on their names, and avoid repetition.

## Stretch goals

- Add logging and exception handling to your JSON-loading code, so that your program fails gracefully in the case where individual transactions are badly formatted or where the file as a whole is badly formatted.
- Similarly, add logging and exception handling to your XML-loading code.
- Add support for a new command, `save <filename>`, which writes the loaded transactions out in a format of your choice.
