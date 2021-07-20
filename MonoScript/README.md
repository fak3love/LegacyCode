# MonoScript
MonoScript is a simple, interpreted programming language.

More information [here](https://github.com/neteromode/LegacyCode/edit/master/MonoScript/wiki).

# Syntax

> **Comments**

```python
# Single comment!
```
```js
/* Multi comment! */
```

> **Imports**

```python
import "C:\Users\ImportUser\FileScript.ms" #Full path to the file
import "FileScript.ms" #If file locate in current directory
import "FileScript" #The file extension can be omitted if it contains the ".ms" extension
```

> **Usings**

```python
using MonoSpace.Test.MyClass #Use this namespace
using MonoSpace.Test.MyClass as myclass #Use myclass like MyClass

#The full namespace must be specified after the using statement
```

> **Modifiers**

| Modifier | Description |
| --- | --- |
| public | Makes the data context public |
| private | Makes the data context private |
| protected | Makes the data context public only to the child |
| static | Makes the data context static |
| readonly | Marks fields with a readonly modifier, you can change such fields when declaring and in the class constructor |
| const | Marks fields with the const modifier, you can change such fields only when declaring |

> **Types**

| Type | Description | Modifiers | May contain | Additionally |
| --- | --- | --- | --- | --- |
| class | Type reference | public, static, private | Classes, Fields, Methods, Overload | Can inherit from another class |
| struct | Type value | public, static, private | Structs, Fields, Methods, Overload | No |
| enum | Type enumeration | public | Enum Values | No |
| def | Execution function | public, private, protected, static | Script | return \<value> |
| var | Variable | public, private, protected, static, const, readonly | Value | No |

> **Classes** | **Structs**

```csharp
class Animal //or struct
{
   var Name
}

class Cat : Animal //or struct, structure cannot be inherited
{
   def Cat(var name) //constructor
   {
      Name = name
   }
}
```

> **Enums**

```csharp
enum Color { Green, White, Yellow, Black }
```

> **Methods**

```python
def Summator(var x, var y)
{
   return x + y
}
```

> **Variables**

```js
var myvar //Variable contains a null value
var varName = "variable value" //Variable contains string value
```

> **Loops and Conditional statements**

| Operator | Type |
| --- | --- |
| for | Loop |
| while | Loop |
| if | Conditional |

| Action Operator | Value | Description | Work with Type | Ignore Type | Example |
| --- | --- | --- | --- | --- | --- |
| break | Any integer | Exiting the loop | for, while | if | break \<1 or max int> |
| continue | None | Starts execution of the next block of the loop | for, while | if | continue |
| quit | Any integer | Exiting the conditional operator | if | for, while | quit \<1 or max int> |
| return | Any value | Returns the value and exits the function | None | for, while, if | return null |

```csharp
while(true) { break }

for (var x = 0, y = "cycle-for", len = y.len(); x < len; x++)
{
   //Cycle body
   //Executes three times

   /* Execute when y equals "-" */
   if (y[x] == "-") { quit 1 //exit from 1 operator if }
}

return 0
```

> **Creating types**

```js
var program = Program() //Calling the Program constructor
var white = Color.White //Get the White value of the Color enumeration
```

> **Operators**

| Operators | Description | Object |
| --- | --- | --- |
| &&, \|\| | Conditional And, Conditional Or | Boolean |
| <, >, <=, >=, !=, == | Equality less, more | Any |
| /, %, *, -, + | Arithmetic | Any |
| --, ++ | Decrement, Increment | Field |
| /=, %=, *=, -=, += | Arithmetic and Assignment | Field |

> **Console**

| Method | Description | Note |
| --- | --- | --- |
| print | Prints a message on the console screen | If flag IsBlocked is false |
| println | Prints a message to the console screen and a newline | If flag IsBlocked is false |
| readln | Reads a string from the keyboard | If flag IsBlocked is false |
| read | Reads the pressed character from the keyboard | If flag IsBlocked is false |
| clear | Clear console screen | If flag IsBlocked is false |

> **Basic methods**

| Method | Like extension | Aliases | Description |
| --- | --- | --- | --- |
| ToString(value) | ToString() | toString | Сonvert object to string |
| ToNumber(value) | ToNumber() | toNumber | Convert object to number |
| ToBoolean(value) | ToBoolean() | ToBool, toBoolean, toBool | Convert object to boolean |
| ToLower(value) | ToLower() | toLower | Convert string to lower string |
| ToUpper(value) | ToUpper() | toUpper | Convert string to upper string |
| Length(value) | Length() | length, len | Get the length of an object |
| Value(value) | No | value | Gets the numeric value of the enumeration |
| Rand(), Rand(maxValue), Rand(minValue, maxValue) | No | rand | Generates a random number in a given range |
| FileWrite | No | fw | Writes text to file |
| FileRead | No | fr | Reads text from file |
| FileWriteBytes | No | fwb | Writes an array of bytes to a file |
| FileReadBytes | No | frb | Reads an array of bytes into a file |
