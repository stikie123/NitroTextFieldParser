# NitroTextFieldParser

## Overview
The **NitroTextFieldParser** is a blazing-fast .NET library for parsing delimited or fixed-width text files. Built for high performance, it minimizes memory usage by leveraging `ReadOnlyMemory<char>` and supports advanced features such as quoted field handling. Whether you're processing CSVs or custom delimited files, NitroTextFieldParser provides robust and efficient parsing.

---

## Benchmark Results
The NitroTextFieldParser outperforms the built-in `TextFieldParser` in terms of speed and memory usage. The following benchmark results demonstrate the performance benefits of using the NitroTextFieldParser when processing 1000Rows x 9Columns CSV data:

| Method                                               | Mean      | Error     | StdDev    | Rank | Gen0      | Gen1     | Gen2    | Allocated |
|----------------------------------------------------- |----------:|----------:|----------:|-----:|----------:|---------:|--------:|----------:|
| NitroTextFieldParser                                 |  1.663 ms | 0.0187 ms | 0.0175 ms |    1 |  306.6406 | 134.7656 |       - |   1.35 MB |
| OldTextFieldParser                                   |  9.543 ms | 0.1640 ms | 0.1534 ms |    2 | 3343.7500 | 421.8750 | 15.6250 |  12.51 MB |

---

## Release Notes 1.1.0v
- New Feature: Added schema validation support for parsing fields with precise data structures. Users can now define custom patterns and types for fields, enabling automated detection of malformed data.

---

## Features
- **High Performance**: Minimal memory footprint with direct memory access.
- **Flexible Delimiters**: Supports single or multiple custom delimiters.
- **Quoted Field Handling**: Handles fields enclosed in quotes with support for escaped quotes.
- **Stream-Based**: Reads data directly from any `Stream`, suitable for large files.
- **Encoding Support**: Works with any character encoding.
- **Schema Validation**: Define custom patterns and types for fields to validate data structures.*

---

## Installation
Include the `NitroTextFieldParser` class in your .NET project or package it into a shared library.

---

## Usage

### 1. Initialize the Parser
Create an instance of the parser with your data stream.

```csharp
using NitroTextFieldParser;

using (var stream = File.OpenRead("data.csv"))
using (var parser = new TextFieldParser(stream))
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

Row 1:
- Name
- Age
- City

Row 2:
- Alice, Bob
- 30
- New York

---

## Custom Encoding
Specify a custom character encoding:
```csharp
using (var parser = new TextFieldParser(stream, Encoding.UTF8))
{
    // Custom encoding setup
}
```

---

## Stream Control
Keep the stream open after parsing:

```csharp
var parser = new TextFieldParser(stream, Encoding.UTF8, true, true);
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

Row 1:
- ID
- Name
- Email

Row 2:
- 1
- John Doe
- john@example.com

Row 3:
- 2
- Jane Smith
- jane@example.com

---

## Why Use NitroTextFieldParser?
- Efficient memory usage for handling large files.
- Flexible support for custom delimiters and quoted fields.
- Easy to integrate into any .NET project.

---

## License

This software is dual-licensed:

1. Custom Open Source License
   You may use this software under the terms of this Custom Open Source License.

2. Commercial License
   For commercial use, a separate license is required. Please contact stianbekker@gmail.com for details.

Custom Open Source License:
-----------------
Copyright (c) [2025] [Christiaan Bekker/Stikie123]

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to use,
copy, modify, merge, and distribute the Software, subject to the following
conditions:

1. The Software may be used freely in:
   - Open-source projects that are free and publicly available.
   - Personal projects or non-commercial educational purposes.

2. For any commercial use, including but not limited to using the Software in
   paid products, services, or proprietary projects, the user must obtain a
   separate commercial license from [Christiaan Bekker/Stikie123]. Please
   contact [stianbekker@gmail.com] to arrange licensing terms.

3. Credit must be given to [Christiaan Bekker/Stikie123] in all copies or
   substantial portions of the Software. This includes clear attribution in
   the documentation, source code, or any materials distributed with the
   Software.

4. Any modifications made to the Software must:
   - Be clearly documented.
   - Be made available to the public under the same terms as this license
     unless explicit written permission is granted by [Christiaan Bekker/Stikie123].

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE, AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES, OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT, OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

Commercial License Text:
------------------------
For commercial use, please acquire a license by contacting [stianbekker@gmail.com].