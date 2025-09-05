using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AspnetCoreMvcFull.Models.Contro_de_Trafico
{
  public enum LugarServicioTipo : byte
  {
    Remoto = 0,
    Presencial = 1
  }
  public class ControlTraficoPostDto
  {
    // Header (checkboxes) - si no se marcan llegan como false
    public bool CanalEmail { get; set; }
    public bool CanalWeb { get; set; }
    public bool CanalPresencial { get; set; }
    public bool CanalTelefono { get; set; }
    public bool CanalChatbot { get; set; }
    public bool? GD { get; set; }
    public bool? SC { get; set; }
    public bool? SID { get; set; }

    // Selects / inputs requeridos
    public string SolicitanteId { get; set; } = "";
    public string? ContratoId { get; set; } = "";
    public string EmpresaId { get; set; } = "";
    public string? AsignadoAId { get; set; }    // puede venir vacío
    public string RazonServicioId { get; set; } = "";

    public string TelefonoServicio { get; set; } = "";
    public string EmailServicio { get; set; } = "";
    public string? DireccionServicio { get; set; }

    // Radios: "0"=Remoto, "1"=Presencial
    public string LugarServicio { get; set; } = "0";

    // Textos a parsear
    public string? FechaProximoServicio { get; set; } // ej: 28/08/2025 ó 08/28/2025
    public string? HoraServicio { get; set; }         // ej: 09:30

    public string? DescripcionIncidente { get; set; }

    // Hidden que mandaste (por si quieres usarlo)
    public string? EstadoFormulario { get; set; }
    public int ticket { get; set; }
    public string? Fecha { get; set; }
    public string? Proveedor { get; set; }
  }

}
