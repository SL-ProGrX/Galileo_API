using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.CuentasxCobrar;
using Galileo_API.Models.ProGrX.CuentasxCobrar;
using System.Collections.Generic;
using static Galileo_API.Models.ProGrX.CuentasxCobrar.FrmCxCClientesContratosModels;

namespace Galileo_API.BusinessLogic.ProGrX.CuentasxCobrar
{
    public class FrmCxCClientesBL
    {
        private readonly FrmCxCClientesDB _db;

        public FrmCxCClientesBL(IConfiguration config)
        {
            _db = new FrmCxCClientesDB(config);
        }

        public ErrorDto<List<CxcPersonaDto>> CxcPersonas_Lista(int codEmpresa, string orden)
        {
            return _db.CxcPersonas_Lista(codEmpresa, orden);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> EstadoCivil_Lista(int codEmpresa)
        {
            return _db.EstadoCivil_Lista(codEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Clasificacion_Lista(int codEmpresa)
        {
            return _db.Clasificacion_Lista(codEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> TiposId_Lista(int codEmpresa)
        {
            return _db.TiposId_Lista(codEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Provincias_Lista(int codEmpresa)
        {
            return _db.Provincias_Lista(codEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Cantones_Lista(int codEmpresa, string provincia)
        {
            return _db.Cantones_Lista(codEmpresa, provincia);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Distritos_Lista(int codEmpresa, string provincia, string canton)
        {
            return _db.Distritos_Lista(codEmpresa, provincia, canton);
        }

        public ErrorDto<CxcPersonaValidaResult?> CxcPersona_Valida(int codEmpresa, string cedula)
        {
            return _db.CxcPersona_Valida(codEmpresa, cedula);
        }

        public ErrorDto<SocioInfoDto?> Socio_Info(int codEmpresa, string cedula)
        {
            return _db.Socio_Info(codEmpresa, cedula);
        }

        public ErrorDto<PersonaInfoDto?> Persona_Info(int codEmpresa, string cedula)
        {
            return _db.Persona_Info(codEmpresa, cedula);
        }

        public ErrorDto<CxcPersonaLargoCedulaResult?> CxcPersona_LargoCedula(int codEmpresa, short tipoId)
        {
            return _db.CxcPersona_LargoCedula(codEmpresa, tipoId);
        }

        public ErrorDto<bool> CxcPersona_Guardar(int codEmpresa, CxcPersonaSaveParams param)
        {
            return _db.CxcPersona_Guardar(codEmpresa, param);
        }

        public ErrorDto<bool> CxcPersona_Eliminar(int codEmpresa, CxcPersonaDeleteParams param)
        {
            return _db.CxcPersona_Eliminar(codEmpresa, param);
        }

        public ErrorDto<List<CxcPersonaCuentaDto>> CxcPersonasCuentas(int codEmpresa, string cedula, string estado)
        {
            return _db.CxcPersonasCuentas(codEmpresa, cedula, estado);
        }

        public ErrorDto<List<ClientesContratosData>> CxcPersonasContratos(int codEmpresa, string cedula)
        {
            return _db.CxcPersonasContratos(codEmpresa, cedula);
        }

        public ErrorDto<List<CxcPersonaContratosPagadorDto>> CxcPersonasContratosPagadores(int codEmpresa, string codContrato, string cedula)
        {
            return _db.CxcPersonasContratosPagadores(codEmpresa, codContrato, cedula);
        }

        public ErrorDto<List<PersonasContratosSuscripcionesData>> CxcPersonasContratosSuscripciones(int codEmpresa, string codContrato, string cedula)
        {
            return _db.CxcPersonasContratosSuscripciones(codEmpresa, codContrato, cedula);
        }

        public ErrorDto<bool> CxcContratoPagador_Eliminar(int codEmpresa, CxcContratoPagadorDeleteParams param)
        {
            return _db.CxcContratoPagador_Eliminar(codEmpresa, param);
        }

        public ErrorDto<bool> CxcContratoSuscripcion_Eliminar(int codEmpresa, CxcContratosSuscripcionDeleteParams param)
        {
            return _db.CxcContratoSuscripcion_Eliminar(codEmpresa, param);
        }

        public ErrorDto<List<CxcCuentaBancariaDto>> CxcCuentasBancarias(int codEmpresa, string cedula)
        {
            return _db.CxcCuentasBancarias(codEmpresa, cedula);
        }
    }
}
