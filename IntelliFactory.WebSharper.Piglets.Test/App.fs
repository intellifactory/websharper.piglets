﻿module App

open IntelliFactory.WebSharper
open IntelliFactory.WebSharper.Piglets

module Model =

    type Name = { firstName: string; lastName: string }

    type User =
        { name: Name; age: int; isMale: bool; comments: string }

        [<JavaScript>]
        static member Pretty u =
            u.name.firstName + " " + u.name.lastName
            + ", aged " + string u.age
            + if u.isMale then ", male" else ", female"
            + if u.comments = "" then ", no comment" else ("; " + u.comments)

module ViewModel =

    open Model

    [<JavaScript>]
    let Name init =
        Piglet.Return (fun f l -> { firstName = f; lastName = l })
        <*> (Piglet.Yield init.firstName
            |> Piglet.Validation.IsNotEmpty "First name should not be empty.")
        <*> (Piglet.Yield init.lastName
            |> Piglet.Validation.IsNotEmpty "Last name should not be empty.")
        |> Piglet.MapViewArgs (fun f l -> (f, l))

    [<JavaScript>]
    let User init =
        Piglet.Return (fun n a m c -> { name = n; age = a; isMale = m; comments = c })
        <*> Name init.name
        <*> (Piglet.Yield init.age
            |> Piglet.Validation.Is (fun a -> a >= 18) "You must be over 18.")
        <*> Piglet.Yield init.isMale
        <*> Piglet.Yield init.comments
        |> Piglet.TransmitReader
        |> Piglet.WithSubmit
        |> Piglet.Run (fun u ->
            JavaScript.Alert (Model.User.Pretty u))

module View =

    open Model
    open IntelliFactory.WebSharper.Html
    module C = Piglet.Controls

    [<JavaScript>]
    let User (firstName, lastName) age isMale comments liveUser submit =
        Div [
            Div [
                Label [Text "First name:"]
                C.Input firstName
            ]
            Div [
                Label [Text "Last name:"]
                C.Input lastName
            ]
            Div [
                C.CheckBox isMale -< [Attr.Id "ismale"]
                Label [Text "Male"; Attr.For "ismale"]
            ]
            Div [
                Label [Text "Age:"]
                C.IntInput age
            ]
            Div [
                Label [Text "Comments:"]
                C.TextArea comments
            ]
            Table [
                TBody [
                    TR [
                        TH [Attr.ColSpan "5"] -< [Text "Summary"]
                    ]
                    TR [
                        TH [Text "First name"]
                        TH [Text "Last name"]
                        TH [Text "Gender"]
                        TH [Text "Age"]
                        TH [Text "Comments"]
                    ]
                    TR [
                        // This one will show up even if other parts are invalid
                        TD |> C.ShowString firstName id
                        // These will only show up if the whole user is valid
                        TD |> C.ShowString liveUser (fun u -> u.name.lastName)
                        TD |> C.ShowString liveUser (fun u -> if u.isMale then "Male" else "Female")
                        TD |> C.ShowString liveUser (fun u -> string u.age)
                        TD |> C.Show liveUser (fun u ->
                            if u.comments = "" then
                                [I [Text "(no comment)"]]
                            else [Span [Text u.comments]])
                    ]
                ]
            ]
            Div |> C.ShowErrors liveUser (fun msgs ->
                [
                    Div [Attr.Style "border:solid 1px #c00;color:#c00;margin:10px;padding:5px"] -< [
                        for m in msgs do
                            yield Text m
                            yield (Br [] :> _)
                    ]
                ])
            Div [
                C.Submit submit
            ]
            Div [
                Br []; Br []
                Label [Text "Age again:"]
                C.IntInput age
                Span [Text "(just to test several inputs connected to the same stream)"]
            ]
        ]

[<JavaScript>]
let UI() =
    let initUser : Model.User =
        {
            name = { firstName = "John"; lastName = "Rambo" }
            age = 40
            isMale = true
            comments = "Badass"
        }
    ViewModel.User initUser
    |> Piglet.Render View.User 