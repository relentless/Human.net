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
let ``when a neuron receives an input at it's threshold it activates``() =
    // arrange
    latch.Reset() |> ignore
    let neuronFired = ref false
    let neuron = createNeuron 10 [(createTestNeuron (fun () -> neuronFired := true))]
    
    // act
    neuron.Post 10

    // assert
    latch.WaitOne(1000) |> ignore
    !neuronFired |> should equal true

// this sometimes fails, which seems to be a timing issue related to the second output neuron
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
    latch.WaitOne(10000) |> ignore
    !neuron1Fired |> should equal true
    !neuron2Fired |> should equal true

[<Fact>]
let ``when the sum of the last 5 inputs is equal to or greater than a neuron's threshold, it activates``() =
    // arrange
    latch.Reset() |> ignore
    let neuronFired = ref false
    let neuron = createNeuron 10 [(createTestNeuron (fun () -> neuronFired := true))]
    
    // act
    neuron.Post 3
    neuron.Post 0
    neuron.Post 2
    neuron.Post 1
    neuron.Post 4

    // assert
    latch.WaitOne(1000) |> ignore
    !neuronFired |> should equal true

// TODO: use only the last 5 values when checking for activation.