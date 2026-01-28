using Galileo.Models.ERROR;
using Galileo.Models.TES;
using Galileo_API.DataBaseTier.TES;

namespace Galileo_API.BusinessLogic.TES
{
    public class FrmTesTokenBL
    {

        private readonly FrmTesTokenDB TokenDB;

        public FrmTesTokenBL(IConfiguration config)
        {
            TokenDB = new FrmTesTokenDB(config);
        }

        public ErrorDto<List<TesTokenDto>> TES_Token_Top_Obtener(int CodEmpresa)
        {
            return TokenDB.TES_Token_Top_Obtener(CodEmpresa);
        }

        public ErrorDto TES_Token_Cerrar(int CodEmpresa, string Id)
        {
            return TokenDB.TES_Token_Cerrar(CodEmpresa,Id);
        }

        public ErrorDto<List<TesTokenSolicitudesData>> TES_Token_Pen_Obtener(int CodEmpresa)
        { 
            return TokenDB.TES_Token_Pen_Obtener(CodEmpresa);
        }

        public ErrorDto TES_Token_Pen_Incluir(int CodEmpresa, string token, List<string> solicitudes)
        {
            return TokenDB.TES_Token_Pen_Incluir(CodEmpresa, token, solicitudes);
        }
        public ErrorDto TES_Token_Crear(int CodEmpresa, string Usuario)
        {
            return TokenDB.TES_Token_Crear(CodEmpresa, Usuario);
        }
    }

}