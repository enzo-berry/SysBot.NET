using System.ComponentModel;

public class ShopifySettings
{
    private const string Startup = nameof(Startup);
    private const string Operation = nameof(Operation);
    private const string Messages = nameof(Messages);
    private const string Lang = nameof(Lang);

    public override string ToString() => "Shopify Integration Settings";

    // Startup

    [Category(Startup), Description("Access token for shopify application.")]
    public string ShopUrl { get; set; } = string.Empty;

    [Category(Startup), Description("Shopify shop url.")]
    public string AccessToken { get; set; } = string.Empty;

    [Category(Lang), Description("Language of the integration, Websocket messages (FR or EN)")]
    public string Language { get; set; } = "FR";

    // Messages for each language
    [Category(Messages), Description("Message when the bot can't queue people.")]
    public string EN_QueueNotAccepting { get; } = "Sorry, I'm not accepting requests at the moment!";

    [Category(Messages), Description("Message lorsque le bot n'accepte pas de demande.")]
    public string FR_QueueNotAccepting { get; } = "Désolé, je n'accepte pas de demande à l'heure actuelle !";

    [Category(Messages), Description("Message when the user is already in the queue.")]
    public string EN_AlreadyInQueue { get; } = "You are already in the queue!";

    [Category(Messages), Description("Message lorsque l'utilisateur est déjà dans la file d'attente.")]
    public string FR_AlreadyInQueue { get; } = "Vous êtes déjà dans la file d'attente !";

    [Category(Messages), Description("Message the user queue position.")]
    public string EN_QueuePosition { get; } = "You are in position {0} in the queue.";

    [Category(Messages), Description("Message la position de l'utilisateur dans la file d'attente.")]
    public string FR_QueuePosition { get; } = "Vous êtes en position {0} dans la file d'attente.";

    [Category(Messages), Description("Message the estimated time to wait.")]
    public string EN_QueueEstimatedTime { get; } = "The estimated time to wait is {0} minutes.";

    [Category(Messages), Description("Message le temps d'attente estimé.")]
    public string FR_QueueEstimatedTime { get; } = "Le temps d'attente estimé est de {0} minutes.";

    [Category(Messages), Description("Message for successful connection.")]
    public string EN_ConnectionSuccess { get; } = "Connected to server.";

    [Category(Messages), Description("Message pour connexion réussie.")]
    public string FR_ConnectionSuccess { get; } = "Connecté au serveur.";

    [Category(Messages), Description("Message for invalid order ID.")]
    public string EN_InvalidOrderID { get; } = "OrderID is invalid.";

    [Category(Messages), Description("Message pour un ID de commande invalide.")]
    public string FR_InvalidOrderID { get; } = "L'ID de commande est invalide.";

    [Category(Messages), Description("Message for failed trade initialization.")]
    public string EN_TradeInitFailed { get; } = "Failed to initialize trade.";

    [Category(Messages), Description("Message pour échec de l'initialisation du trade.")]
    public string FR_TradeInitFailed { get; } = "Échec de l'initialisation du trade.";

    [Category(Messages), Description("Message for error parsing Showdown set.")]
    public string EN_ShowdownParseError { get; } = "{0}: Unable to parse Showdown Set:\n{1}";

    [Category(Messages), Description("Message pour erreur de parsing du set Showdown.")]
    public string FR_ShowdownParseError { get; } = "{0} : Impossible de parser le set Showdown :\n{1}";

    [Category(Messages), Description("Message pour erreur de timeout.")]
    public string EN_TimeoutError { get; } = "That {0} set took too long to generate.";

    [Category(Messages), Description("Message pour erreur de timeout.")]
    public string FR_TimeoutError { get; } = "Ce set de {0} a pris trop de temps à générer.";

    [Category(Messages), Description("Message for version mismatch error.")]
    public string EN_VersionMismatchError { get; } = "Request refused: PKHeX and Auto-Legality Mod version mismatch.";

    [Category(Messages), Description("Message pour erreur de version mismatch.")]
    public string FR_VersionMismatchError { get; } = "Requête refusée : incompatibilité de version entre PKHeX et Auto-Legality Mod.";

    [Category(Messages), Description("Message for general error.")]
    public string EN_GeneralError { get; } = "I wasn't able to create a {0} from that set.";

    [Category(Messages), Description("Message pour erreur générale.")]
    public string FR_GeneralError { get; } = "Je n'ai pas pu créer un {0} à partir de ce set.";

    [Category(Messages), Description("Message for trade canceled.")]
    public string EN_TradeCanceled { get; } = "Trade canceled, {0}";

    [Category(Messages), Description("Message pour trade annulé.")]
    public string FR_TradeCanceled { get; } = "Trade annulé, {0}";

    [Category(Messages), Description("Message for trade finished.")]
    public string EN_TradeFinished { get; } = "Trade finished. Enjoy your {0}!";

    [Category(Messages), Description("Message pour trade terminé.")]
    public string FR_TradeFinished { get; } = "Trade terminé. Profitez de votre {0} !";

    [Category(Messages), Description("Message for trade finished (generic).")]
    public string EN_TradeFinishedGeneric { get; } = "Trade finished!";

    [Category(Messages), Description("Message pour trade terminé (générique).")]
    public string FR_TradeFinishedGeneric { get; } = "Trade terminé !";

    [Category(Messages), Description("Message for trade initialization.")]
    public string EN_TradeInitialize { get; } = "Your trade code will be : {0:0000 0000}. Wait for my signal before joining.";

    [Category(Messages), Description("Message pour initialisation du trade.")]
    public string FR_TradeInitialize { get; } = "Votre code de trade sera : {0:0000 0000}. Attends mon signal pour rejoindre.";

    [Category(Messages), Description("Message for trade searching.")]
    public string EN_TradeSearching { get; } = "I'm waiting for you ! My IGN is {0}.";

    [Category(Messages), Description("Message pour recherche de trade.")]
    public string FR_TradeSearching { get; } = "Je vous attends ! Mon IGN est {0}.";

    [Category(Messages), Description("Message for sending PKM details.")]
    public string EN_SendPKMDetails { get; } = "Here's what you traded me!";

    [Category(Messages), Description("Message pour l'envoi des détails du PKM.")]
    public string FR_SendPKMDetails { get; } = "Voici ce que vous m'avez échangé !";
}
