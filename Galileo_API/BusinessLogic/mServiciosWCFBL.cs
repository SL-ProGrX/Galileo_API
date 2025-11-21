// using Galileo.DataBaseTier;
// using Galileo.Models.ERROR;

// namespace Galileo.BusinessLogic
// {
//     public class mServiciosWCFBL
//     {
//         private readonly IConfiguration _config;

//         public mServiciosWCFBL(IConfiguration config)
//         {
//             _config = config;
//         }

//         public ErrorDto fxValidacionSinpe(int CodEmpresa, string solicitud, string usuario)
//         {
//             return new mServiciosWCFDB(_config).fxValidacionSinpe(CodEmpresa, solicitud, usuario);
//         }

//         public ErrorDto<bool> GenerarFacturacionElectronica(int CodEmpresa,
//             string pCedula,
//             string pNumeroDocumento, string pTipoDoc,
//             byte pTipoDocEletronico, string pNotas, string pTipoTramite)
//         {
//             return new mServiciosWCFDB(_config).GenerarFacturacionElectronica(CodEmpresa, pCedula, pNumeroDocumento, pTipoDoc, pTipoDocEletronico, pNotas, pTipoTramite);
//         }

//         public ErrorDto fxTesEmisionSinpeCreditoDirecto(int CodEmpresa,
//            int Nsolicitud, DateTime vfecha, string vUsuario, int doc_base, int contador)
//         {
//             return new mServiciosWCFDB(_config).fxTesEmisionSinpeCreditoDirecto(CodEmpresa, Nsolicitud, vfecha, vUsuario, doc_base, contador);
//         }

//         public ErrorDto fxTesEmisionSinpeTiempoReal(int CodEmpresa, int Nsolicitud, DateTime vfecha, string vUsuario, int doc_base, int contador)
//         {
//             return new mServiciosWCFDB(_config).fxTesEmisionSinpeTiempoReal(CodEmpresa, Nsolicitud, vfecha, vUsuario, doc_base, contador);
//         }
//     }
// }
