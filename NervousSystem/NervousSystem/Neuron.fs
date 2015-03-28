module Neuron

open System

#nowarn "40"
// activationThreshold
let createNeuron activationThreshold (outputNeurons : MailboxProcessor<int> list) = 
    MailboxProcessor.Start (fun inbox -> 
       // the message processing function
        let rec messageLoop = async{
            let! inputSignal = inbox.Receive()

            if inputSignal >= activationThreshold then
                outputNeurons |> List.iter (fun neuron -> neuron.Post inputSignal)

            return! messageLoop  
            }

        messageLoop 
    )