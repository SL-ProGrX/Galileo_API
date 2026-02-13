
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.CuentasxCobrar;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Galileo_API.Models.ProGrX.CuentasxCobrar.FrmCxCClientesContratosModels;

namespace Galileo_API.Controllers.ProGrX.CuentasxCobrar
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCxCClientesContratosController : ControllerBase
    {
        private readonly FrmCxCClientesContratosBL _bl;

        public FrmCxCClientesContratosController(IConfiguration config)
            => _bl = new FrmCxCClientesContratosBL(config);

        [Authorize]
        [HttpGet("CxCClientesPersonas_Contratos_Consultar")]
        public ErrorDto<string> CxCClientesPersonas_Contratos_Consultar(int codEmpresa, string cedula, int orden, string contrato = "")
        {
            return _bl.CxCClientesPersonas_Contratos_Consultar(codEmpresa, cedula, orden, contrato);
        }

        [Authorize]
        [HttpGet("CxCContratos_Consultar")]
        public ErrorDto<ClientesContratosData> CxCContratos_Consultar(int codEmpresa, string cedula, string contrato)
        {
            return _bl.CxCContratos_Consultar(codEmpresa, cedula, contrato);
        }

        [Authorize]
        [HttpPost("CxCClientesPersonasContratos_Guardar")]
        public ErrorDto CxCClientesPersonasContratos_Guardar(int codEmpresa, string usuario, [FromBody]  ClientesContratosData datos)
        {
            return _bl.CxCClientesPersonasContratos_Guardar(codEmpresa, usuario, datos);
        }

        [Authorize]
        [HttpDelete("CxCClientesPersonas_Contratos_Eliminar")]
        public ErrorDto CxCClientesPersonas_Contratos_Eliminar(int codEmpresa, string usuario, string cedula, string contrato)
        {
            return _bl.CxCClientesPersonas_Contratos_Eliminar(codEmpresa, usuario, cedula, contrato);
        }

        [Authorize]
        [HttpDelete("CxCClientesPersonas_ContratosSuscripciones_Eliminar")]
        public ErrorDto CxCClientesPersonas_ContratosSuscripciones_Eliminar(int CodEmpresa, string usuario, string cedula, string contrato, string cargo)
        {
            return _bl.CxCClientesPersonas_ContratosSuscripciones_Eliminar(CodEmpresa, usuario, cedula, contrato, cargo);
        }

        [Authorize]
        [HttpPost("CxCClientesPersonas_ContratosSuscripciones_Insertar")]
        public ErrorDto CxCClientesPersonas_ContratosSuscripciones_Insertar(int CodEmpresa, string usuario, [FromBody] PersonasContratosSuscripcionesData datos)
        {
            return _bl.CxCClientesPersonas_ContratosSuscripciones_Insertar(CodEmpresa, usuario, datos);
        }

        [Authorize]
        [HttpDelete("CxCClientesPersonas_ContratosPagadores_Eliminar")]
        public ErrorDto CxCClientesPersonas_ContratosPagadores_Eliminar(int CodEmpresa, string usuario, string cedula, string contrato, string cedula_pagador)
        {
            return _bl.CxCClientesPersonas_ContratosPagadores_Eliminar(CodEmpresa, usuario, cedula, contrato, cedula_pagador);
        }

        [Authorize]
        [HttpPost("CxCClientesPersonas_ContratosPagadores_Insertar")]
        public ErrorDto CxCClientesPersonas_ContratosPagadores_Insertar(int CodEmpresa, string usuario, PersonasContratosPagadoresData datos)
        {
            return _bl.CxCClientesPersonas_ContratosPagadores_Insertar(CodEmpresa, usuario, datos);
        }

        [Authorize]
        [HttpGet("CxCClientesPersonas_ContratosPagadores_Lista")]
        public ErrorDto<List<PersonasContratosPagadoresData>> CxCClientesPersonas_ContratosPagadores_Lista(int CodEmpresa, string cedula, string contrato)
        {
            return _bl.CxCClientesPersonas_ContratosPagadores_Lista(CodEmpresa, cedula, contrato);
        }

           
        [Authorize]
        [HttpGet("CxCClientesPersonas_ContratosSuscripcion_Lista")]
        public ErrorDto<List<PersonasContratosSuscripcionesData>> CxCClientesPersonas_ContratosSuscripcion_Lista(int CodEmpresa, string cedula, string contrato)
        {
            return _bl.CxCClientesPersonas_ContratosSuscripcion_Lista(CodEmpresa, cedula, contrato);
        }

        [Authorize]
        [HttpGet("CxC_Contratos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CxC_Contratos_Obtener(int CodEmpresa)
        {
            return _bl.CxC_Contratos_Obtener(CodEmpresa);
        }
    }

}