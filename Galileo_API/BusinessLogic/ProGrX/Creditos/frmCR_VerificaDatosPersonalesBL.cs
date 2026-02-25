using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;

namespace Galileo_API.BusinessLogic.ProGrX.Creditos
{
    public class FrmCRVerificaDatosPersonalesBL
    {
        private readonly FrmCRVerificaDatosPersonalesDB Db;

        public FrmCRVerificaDatosPersonalesBL(IConfiguration config)
        {
            Db = new FrmCRVerificaDatosPersonalesDB(config);
        }

        public ErrorDto<CrVerificaDatosCompletoDto> CR_VerificaDatos_Completo_Obtener(int CodEmpresa, string identificacion)
        {
            return Db.CR_VerificaDatos_Completo_Obtener(CodEmpresa, identificacion);
        }
        public ErrorDto<List<DropDownListaGenericaModel>> CR_VerificaDatos_Persona_F4_Obtener(int CodEmpresa, string? filtro)
        {
            return Db.CR_VerificaDatos_Persona_F4_Obtener(CodEmpresa, filtro);
        }
        public ErrorDto<CrVerificaDatosListaResult<CrVerificaDatosNombramientoItem>> CR_VerificaDatos_Nombramientos_Lista_Obtener(int CodEmpresa, string identificacion, string parametros)
        {
            return Db.CR_VerificaDatos_Nombramientos_Lista_Obtener(CodEmpresa, identificacion, parametros);
        }
        public ErrorDto<CrVerificaDatosListaResult<CrVerificaDatosNombramientoItem>> CR_VerificaDatos_Nombramientos_Lista_Export(int CodEmpresa, string identificacion, string parametros)
        {
            return Db.CR_VerificaDatos_Nombramientos_Lista_Export(CodEmpresa, identificacion, parametros);
        }
        public ErrorDto CR_VerificaDatos_Nombramiento_Agregar(int CodEmpresa, CrVerificaDatosNombramientoAgregarRequest req)
        {
            return Db.CR_VerificaDatos_Nombramiento_Agregar(CodEmpresa, req);
        }
        public ErrorDto CR_VerificaDatos_Guardar(int CodEmpresa, CrVerificaDatosGuardarRequest req)
        {
            return Db.CR_VerificaDatos_Guardar(CodEmpresa, req);
        }
        public ErrorDto<List<DropDownListaGenericaModel>> CR_EstadoLaboral_Dropdown_Obtener(int CodEmpresa)
        {
            return Db.CR_EstadoLaboral_Dropdown_Obtener(CodEmpresa);
        }
        public ErrorDto<List<DropDownListaGenericaModel>> CR_EstadoCivil_Dropdown_Obtener(int CodEmpresa)
        {
            return Db.CR_EstadoCivil_Dropdown_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CR_Nacionalidades_Dropdown_Obtener(int CodEmpresa)
        {
            return Db.CR_Nacionalidades_Dropdown_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CR_Provincias_Dropdown_Obtener(int CodEmpresa)
        {
            return Db.CR_Provincias_Dropdown_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CR_Cantones_Dropdown_Obtener(int CodEmpresa, string provincia)
        {
            return Db.CR_Cantones_Dropdown_Obtener(CodEmpresa, provincia);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CR_Distritos_Dropdown_Obtener(int CodEmpresa, string provincia, string canton)
        {
            return Db.CR_Distritos_Dropdown_Obtener(CodEmpresa, provincia, canton);
        }
        public ErrorDto CR_VerificaDatos_Catalogo_Asignar(int CodEmpresa, CrVerificaDatosAsignarCatalogoRequest req)
        {
            return Db.CR_VerificaDatos_Catalogo_Asignar(CodEmpresa, req);
        }
    }
}