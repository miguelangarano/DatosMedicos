# DatosMedicos

Este proyecto sirve para tomar datos de una base de datos en Access y después subirlo a mLab que es una base de datos MongoDB en la nube.  

-Para utilizar el proyecto se debe abrir con visual studio y confirmar que se tienen instalados los siguientes paquetes mediante NuGet:  
  *MongoDB Driver  
  *Newtonsoft Json  

-La forma de uso que se debe seguir es, primero ingresar el IDRegistro que es el identificador de la tabla CabeceraRegistro.  
-Si se ingresa un ID válido, aparecerán los datos del paciente y se habilitará el botón para subir a mLab todos los datos relacionados con esta CabeceraRegistro.  
