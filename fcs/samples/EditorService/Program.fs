﻿// Open the namespace with InteractiveChecker type
open System
open Microsoft.FSharp.Compiler
open Microsoft.FSharp.Compiler.Range
open Microsoft.FSharp.Compiler.SourceCodeServices
open Microsoft.FSharp.Compiler.QuickParse

// Create an interactive checker instance (ignore notifications)
let checker = FSharpChecker.Create()

let parseWithTypeInfo (file, input) = 
    let checkOptions, _errors = checker.GetProjectOptionsFromScript(file, input) |> Async.RunSynchronously
    let parsingOptions, _errors = checker.GetParsingOptionsFromProjectOptions(checkOptions)
    let untypedRes = checker.ParseFile(file, input, parsingOptions) |> Async.RunSynchronously
    
    match checker.CheckFileInProject(untypedRes, file, 0, input, checkOptions) |> Async.RunSynchronously with 
    | FSharpCheckFileAnswer.Succeeded(res) -> untypedRes, res
    | res -> failwithf "Parsing did not finish... (%A)" res

// ----------------------------------------------------------------------------
// Example
// ----------------------------------------------------------------------------

let input = 
  """
  let foo() = 
    let msg = "Hello world"
    if true then 
      printfn "%s" msg.
  """
let inputLines = input.Split('\n')
let file = "/home/user/Test.fsx"

let identTokenTag = FSharpTokenTag.Identifier
let untyped, parsed = parseWithTypeInfo (file, input)
// Get tool tip at the specified location
let tip = parsed.GetTooltipText(mkPos 2 7, inputLines.[1], identTokenTag)

printfn "%A" tip

// Get declarations (autocomplete) for a location
let decls = 
    parsed.GetCompletionItems(Some untyped, mkPos 5 23, inputLines.[4]) 
    |> Async.RunSynchronously

for item in decls.Items do
    printfn " - %s" item.Name
