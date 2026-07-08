namespace AuthApi.Controllers;

[ApiController]
[Route("transaction/v1")]
[Authorize]
[ApiExplorerSettings(GroupName = "v1")]
public class TransactionController : ControllerBase
{
    // [GET] /transaction/v1/{id}
    // [POST] /transaction/v1/transfer
        // Header X-TransactionType: [Enum] TipoTransacoes
        // logica para implementar se for despesa, receita ou transferencia
    // [PUT] /transaction/v1/transfer/{id}
    // [GET] /transaction/v1/history
    // [DELETE] /transaction/v1/{id}

    // [GET] /transaction/v1/exchange/{id}
    // [POST]  /transaction/v1/exchange/
        // Header X-TipoTransacaoBolsa: [Enum] TipoTransacaoBolsa
        // logica para implementar se for sell ou buy
    // [PUT] /transaction/v1/exchange/{id}
    // [GET] /transaction/v1/exchange/history
    // [DELETE] /transaction/v1/exchange/{id}


    
}