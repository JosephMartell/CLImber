# CLImber - A Command Line Interface Library [![Build and Test](https://github.com/JosephMartell/CLImber/actions/workflows/BuildAndTest.yml/badge.svg?branch=main)](https://github.com/JosephMartell/CLImber/actions/workflows/BuildAndTest.yml)

## Introduction
CLImber is a .Net library aimed at offloading the work of setting up, documenting, and parsing command line arguments. CLImber should allow your code to be smaller and more concisely focused on the actual work at hand rather than the plumbing required to support a robust command line interface.

CLImber was motivated by writing argument parsing logic over and over again for small utility projects. The need was highlighted as the functional code in my projects was dwarfed by the code required to parse and handle more and more options.

## Technologies
My goal with CLImber is to keep it as small and as portable as possible.
 * .NET Standard 2.0 & C# v7.3

That's it. There are no other libraries, dependencies, or technologies. It should be noted that CLImber does use reflection to do most of its work.

## Setup
Search for JM.CLImber on NuGet

## Use
CLImber defines 3 types of command line artifacts. These are inspired by the git command line, so we will use that as an example:

`git checkout -b new_branch`

`checkout` is a command. Commands are considered the primary element that actually does work. In this case we know that `checkout` is going to switch our working directory to another branch.

`-b` is an option. Options are used to modify the behavior of commands. `-b` is telling the `checkout` command to create a new branch.

`new_branch` is an argument. An argument provides additional information to the command so it can complete a task. This example is providing `checkout` with the name of the branch to create and then checkout.


## Implementation
CLImber maps each of the command line arguments to specific code elements. Commands map to classes; Options map to properties; and arguments map to method parameters;

The following is an example of a class that would handle the `checkout` command detailed previously:
```c#
using CLImber;

namespace CLImber.Example
{
    [CommandClass("checkout")]
    public class CheckoutCommand
    {
        [CommandOption("new-branch", Abbreviation='b']
        public bool NewBranch { get; set; }
        
        [CommandHandler]
        public void Checkout(string branchName)
        {
            ///Do the actual work here.
        }
    }
}
```

### Register the `checkout` command with CLImber
At runtime, CLImber uses reflection to scan the codebase and find all classes decorated with the `CommandClass` attribute. In our code example we have decorated our CheckoutCommand class with the `CommandClass` attribute and provided a "checkout" argument. This argument determines the name of the command that CLImber will associate with this class.
```c#
    ...
    [CommandClass("checkout")]
    public class CheckoutCommand
    {
    ...
```
### Create the new-branch option
The git checkout command accepts an option to create a new branch instead of switching to an existing branch. To implement this in CLImber we create a property in our CheckoutCommand class:

```c#
    ...
        [CommandOption("new-branch", Abbreviation='b']
        public bool NewBranch { get; set; }
    ...
```
By decorating our property with the `CommandOption` attribute CLImber will recognize that this property is an option for the checkout command. The first argument is the full option name which would be invoked by using `--new-branch ` on the command line. The `Abbreviation` parameter designates the character used to invoke this option using abbreviated syntax. On the command line this would look like: `-b`.

### Create the command handler method
We have already flagged the `CheckoutCommand` class as representing the `checkout` command. But we haven't told CLImber that we are expecting a single string argument to be provided when the `checkout` command is invoked. To do this we add a `CommandHandler` method to our class like this:

```c#
    ...
        [CommandHandler]
        public void Checkout(string branchName)
        {
            ///Do the actual work here.
        }
    ...
```
When the `checkout` command is invoked CLImber will scan the `CheckoutCommand` class for `CommandHandler` methods. Assuming a branch name was provided CLImber will find all `CommandHandler` methods that accept 1 argument and then try to invoke those methods. In this case because our Checkout method accepts a single string argument CLImber would pass the branch name provided by the user. 


### Let CLImber Handle your CLI
Now that we've created this class and properly designated the options and handler methods all that is left to do is tell our program that CLImber needs to handle our CLI arguments. In its simplest form:

```c#
static void Main(string[] args)
{
    (new CLIHandler()).Handle(args);
}

```

## Summary
This is a simple example of implementing a command using CLImber, but CLImber can do more. Methods can have arguments other than `string` and CLImber will attempt to convert to the appropriate type. Arrays can be used in Handler methods to handle any number of arguments. CLImber also includes a rudimentary dependency injection system. CLImber can also enumerate commands, options, and arguments so createing relevant on-screen help is simpler too.  Documentation for all these features will be available in the wiki, as soon as possible.

With simple attributes and a little bit of reflection CLImber handles the discovery of commands and the methods to call to make sure everything is handled correctly. Your project code remains cleaner and focused on achieving the operational goals.
