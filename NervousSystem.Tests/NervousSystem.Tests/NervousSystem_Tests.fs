module NervousSystem.Tests

open Xunit
open FsUnit.Xunit
open Neuron
open System.Threading

let latch = new AutoResetEvent(false)

#nowarn "40"
let createTestNeuron output = 
    MailboxProcessor.Start (fun inbox -> 
        let rec messageLoop = async{
        
            let! msg = inbox.Receive()
            output()
            latch.Set() |> ignore
        }

        messageLoop 
    )

[<Fact>]
let ``when a neuron receives an input at it's threshold it sends a signal to the output neuron``() =
    // arrange
    latch.Reset() |> ignore
    let neuronFired = ref false
    let neuron = createNeuron 10 [(createTestNeuron (fun () -> neuronFired := true))]
    
    // act
    neuron.Post 10

    // assert
    latch.WaitOne(100) |> ignore
    !neuronFired |> should equal true

[<Fact>]
let ``when a neuron activates it sends a signal to all output neurons``() =
    // arrange
    latch.Reset() |> ignore
    let neuron1Fired = ref false
    let neuron2Fired = ref false
    let neuron = 
        createNeuron 1 [
            (createTestNeuron (fun () -> neuron1Fired := true))
            (createTestNeuron (fun () -> neuron2Fired := true))]
    
    // act
    neuron.Post 1

    // assert
    latch.WaitOne(100) |> ignore
    !neuron1Fired |> should equal true
    !neuron2Fired |> should equal true

[<Fact>]
let ``when neuron receives an input under it's threshold it doesn't activate``() =
    // arrange
    let threshold = 10
    latch.Reset() |> ignore
    let neuronFired = ref false
    let neuron = createNeuron 10 [(createTestNeuron (fun () -> neuronFired := true))]
    
    // act
    neuron.Post 9

    // assert
    latch.WaitOne(100) |> ignore
    !neuronFired |> should equal false