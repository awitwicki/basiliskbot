  module Program
  
  open System
  open Funogram.Telegram.Bot
  open Funogram.Api
  open ExtCore.Control
  open Funogram.Telegram.Api
  
  let readLines (filePath:string) = System.IO.File.ReadAllLines(filePath)
  let shuffle next xs = xs |> Seq.sortBy (fun _ -> next())
  let responses = readLines "dataset.txt" 

  let getRandomResponse() =
      let rnd = Random()
      responses 
      |> shuffle (fun _ -> rnd.Next()) 
      |> Seq.head

  /// Handler for voice messages
  let onVoice context =
    maybe {
      let response = getRandomResponse()
      let! message = context.Update.Message
      
      Some message.MessageId
      |> sendMessageReply message.Chat.Id response
      |> api context.Config
      |> Async.Ignore
      |> Async.Start
      }
      |> ignore
                  
  /// Handler for "/start" command
  let onStart context =
    maybe {
      let! message = context.Update.Message
      let response = "Добавь меня в чат и я буду бороться с войсами"

      Some message.MessageId
      |> sendMessageReply message.Chat.Id response
      |> api context.Config
      |> Async.Ignore
      |> Async.Start
    } |> ignore

  /// Check message is voice and run handler
  let voiceCheck (handler: UpdateContext -> unit) (context: UpdateContext) =
    context.Update.Message
    |> Option.bind (fun message -> message.Voice)
    |> Option.map (fun _ -> handler context)
    |> Option.isSome
    |> not

  /// Handles all Telegram events.
  let update context =
    processCommands context [ 
        cmd "/start" onStart 
        voiceCheck onVoice 
    ]
    |> ignore
  
  [<EntryPoint>]
  let main _ =
    startBot {
      defaultConfig with
        Token =  System.Environment.GetEnvironmentVariable("BASILISKBOT_TELEGRAM_TOKEN")
    } update None
    |> Async.RunSynchronously
    |> ignore
    0
