# CLImber - A Command Line Interface Library

## Introduction
CLImber is a .Net library aimed at offloading the work of setting up, documenting, and parsing command line arguments. CLImber should allow your code to be smaller and more concisely focused on the actual work at hand rather than the plumbing required to support a robust command line interface.

CLImber was motivated by writing argument parsing logic over and over again for small utility projects. The need was highlighted as the functional code in my projects was dwarfed by the code required to parse and handle more and more options.

## Technologies
My goal with CLImber is to keep it as small and as portable as possible.
 * .NET Standard 2.0 & C# v7.3

That's it. There are no other libraries, dependencies, or technologies. It should be noted that CLImber does use reflection to do most of its work.

## Setup
CLImber will be a published NuGet package. There are a few more features to complete before publishing though.

## Use
CLImber defines 3 types of command line artifacts. These are inspired by the git command line, so we will use that as an example:

`git checkout -b new_branch`

`checkout` is a command. Commands are considered the primary element that actually does work. In this case we know that `checkout` is going to switch our working directory to another branch.

`-b` is an option. Options are used to modify the behavior of commands. `-b` is telling the `checkout` command to create a new branch
> CLImber does not currently support options. It is included here to make the explanation complete and because it is a planned feature. More details will be provided as that feature nears completion.

`new_branch` is an argument. An argument provides additional information to the command so it can complete a task. This example is providing `checkout` with the name of the branch to create and then checkout.


## Implementation
So how would you tell CLImber about a checkout command? CLImber uses reflection to examine your code and find classes and their members that you have designated as commands. You have to provide the command string when you decorate your class with the `CommandClass` attribute.

```c#
using CLImber;

namespace CLImber.Example
{
    [CommandClass("checkout")]
    public class CheckoutCommand
    {
        [CommandHandler]
        public void Checkout(string branchName)
        {
            ///Do the actual work here.
        }
    }
}
```
CLImber can then be invoked at your Main method:

```c#
static void Main(string[] args)
{
    (new CLIHandler()).Handle(args);
}

```

CLImber then examines the assembly to find the `CommandClass` class with the correct command string. It then finds the methods in that class that have been decorated with the `CommandHandler` attribute. If there are multiple then CLImber will pick the method that has the correct number of arguments. If any arguments require conversion CLImber will convert them and then construct the class and invoke the method, passing all arguments. This means that your methods can use strongly typed arguments instead of having to convert from strings as the first step.

With simple attributes and a little bit of reflection CLImber handles the discovery of commands and the methods to call to make sure everything is handled correctly. Your project code remains cleaner and focused on achieving the operational goals.

The list of type converters that CLImber is aware of can be extended so you can deal with any classes that are particular to your libraries. CLImber even supports rudimentary dependency injection so you don't have to have a bunch of global resources in your project either.
