# NitroTextFieldParser

## Overview
The **NitroTextFieldParser** is a blazing-fast .NET library for parsing delimited or fixed-width text files. Built for high performance, it minimizes memory usage by leveraging `ReadOnlyMemory<char>` and supports advanced features such as quoted field handling. Whether you're processing CSVs or custom delimited files, NitroTextFieldParser provides robust and efficient parsing.

---

## Features
- **High Performance**: Minimal memory footprint with direct memory access.
- **Flexible Delimiters**: Supports single or multiple custom delimiters.
- **Quoted Field Handling**: Handles fields enclosed in quotes with support for escaped quotes.
- **Stream-Based**: Reads data directly from any `Stream`, suitable for large files.
- **Encoding Support**: Works with any character encoding.

---

## Installation
Include the `NitroTextFieldParser` class in your .NET project or package it into a shared library.

---

## Usage

### 1. Initialize the Parser
Create an instance of the parser with your data stream.

```csharp
using (var stream = File.OpenRead("data.csv"))
using (var parser = new NitroTextFieldParser(stream))
{
    parser.SetDelimiters(","); // Set the delimiter for parsing
    parser.HasFieldsEnclosedInQuotes = true; // Enable quote handling
}
```

---

### 2. Set Delimiters
Define the delimiters used to separate fields:

```csharp
parser.SetDelimiters(",", "\t"); // Supports multiple delimiters
```

---

### 3. Parse Data'
Read and parse fields line by line:
```csharp
while (!parser.EndOfData)
{
    var fields = parser.ReadFields(); // Returns ReadOnlyMemory<char>[] for efficient access
    foreach (var field in fields)
    {
        Console.WriteLine(field.ToString()); // Convert to string for display
    }
}
```

---

### 4. Handle Quoted Fields
Enable quote handling for fields:
```csharp
parser.HasFieldsEnclosedInQuotes = true; // Enable quote handling
```
Example Input:
```arduino
"Name","Age","City"
"Alice, Bob","30","New York"
```
Output:
- **Name
- **Age
- **City
- **Alice, Bob
- **30
- **New York

---

## Custom Encoding
Specify a custom character encoding:
```csharp
using (var parser = new NitroTextFieldParser(stream, Encoding.UTF8))
{
    // Custom encoding setup
}
```

---

## Stream Control
Keep the stream open after parsing:

```csharp
var parser = new NitroTextFieldParser(stream, Encoding.UTF8, true, true);
```

---

## Read Full Content
Read the entire file content at once:
```csharp
var content = parser.ReadToEnd();
```

---

## Example Input and Output
Input File (data.csv):

```csv
"ID","Name","Email"
1,"John Doe","john@example.com"
2,"Jane Smith","jane@example.com"
```
Output:
- **ID
- **Name
- **Email
- **1
- **John Doe
- **john@example.com
- **2
- **Jane Smith
- **jane@example.com

---

## Why Use NitroTextFieldParser?
- **Efficient memory usage for handling large files.
- **Flexible support for custom delimiters and quoted fields.
- **Easy to integrate into any .NET project.

---

## License

