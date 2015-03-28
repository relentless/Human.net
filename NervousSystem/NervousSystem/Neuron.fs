module Neuron

open System

#nowarn "40"
// activationThreshold
let createNeuron activationThreshold (outputNeurons : MailboxProcessor<int> list) = 
    MailboxProcessor.Start (fun inbox -> 
       // the message processing function
        let rec messageLoop previousSignals = async{

            let! inputSignal = inbox.Receive()
            
            let recentSignals = List.append previousSignals [inputSignal] 

            let totalSignal = List.sum recentSignals
            if totalSignal >= activationThreshold then
                outputNeurons |> List.iter (fun neuron -> neuron.Post totalSignal)

            return! messageLoop recentSignals
            }

        messageLoop []
    )