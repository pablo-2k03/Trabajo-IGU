   M S B A M L   `   `   `  ����E  APactometro, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null'clr-namespace:Pactometro
Pactometro  5   	       Pactometro.UpdateData   5   	   W SPresentationCore, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35H 9http://schemas.microsoft.com/winfx/2006/xaml/presentation      5   	   R NSystem.Xaml, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   *localclr-namespace:Pactometro     5   	     � Title$!  Modificar datos electorales��5   	   $	��851��6-   $	��800��6:   $	��800��5	   	   $	��800��6    � Icon 5
      �� 5   
    �� 	UriSource$< 6pack://application:,,,/Pactometro;component/updata.ico?�6   .��5      �� $
��Auto=�6   .��5   
   � -   5        $
��grid��5       � ColumnDefinitions 5      �� 5      �� 5       � RowDefinitions 5      �� 5      $
��Auto �6    �� 5      $
��Auto �6    �� 5      $
��Auto �6    �� 5      $
��Auto �6    �� 5      $
��Auto �6    .+�5      �� $��Modificar Datos��6   $��Center=�6.   $��30(�6K   $
��Bold"�6Y   $��	Condensed&�6k   $��0��6�   $��2��6�   �� 5       $��0��6   $��70q�6)   .+�5!      ��  �� Orientation$ Vertical=�6   
�������.+�5"      �� $��Datos Actuales��6   $��16(�65   $
��Bold"�6C   $��	Condensed&�6U   �� 5#      $��Tipo de elecciones��6   $��11(�69   $
��Bold"�6G   $��	Condensed&�6Y   �� 5$      -   5        $��
elecActual��5$       $	��690��62   $��
10 10 0 10q�6>   �� 5%      $��Fecha��6   $��11(�6,   $
��Bold"�6:   $��	Condensed&�6L   �� 5&      -   5        $��fechaActual��5&       $	��690��63   $��
10 10 0 10q�6?   �� 5'      $��Partidos��6   $��11(�6/   $
��Bold"�6=   $��	Condensed&�6O   �� 5(      -   5        $��partidosAct��5(   R   $	��184��6   $	��100��6*   $
��Left=�67   $��
10 10 0 10q�6e   
�������6y   5)      5*      �� 5-      $��1��6   $��70q�6)   .+�5.      �� $ Vertical=�6   .+�5/      �� $��Datos Nuevos��6   $��16(�63   $
��Bold"�6A   $��	Condensed&�6S   �� 50      $��Tipo de elecciones��6   $��11(�69   $
��Bold"�6G   $��	Condensed&�6Y   �� 51      -   5        $��tipoElecciones��51      $	��177��65   $
��Left=�6A   $��
10 10 0 10q�6\   .@�52      �� -   5        $��	generales��52   '   $��	Generales��68   �� 53      -   5        $��autonomicas��53   '   .��6Z   Autonómicas6g   54      �� 55      -   5         ��Name$ 	comunidad��55   !   $
��Left=�64   $	��Top=�6O   $��10q�6g   �� 56      $��Fecha��6   $��11(�6,   $
��Bold"�6:   $��	Condensed&�6L   &   System.Windows.Controls.Calendar  57      -	   5        $��
fechaNueva��57   +   $	��184��6   $
��Left=�6=   $��
10 10 0 10q�6X   �� 58      $��Partidos��6   $��11(�6/   $
��Bold"�6=   $��	Condensed&�6O   �� 5:      $��Nuevo Partido��6    $��
10 10 0 10q�65   $	��184��6I   $
��Left=�6U   �� 5;      -
   5        $��nombre��5;      $��
10 10 0 10q�6-   $	��184��6B   $
��Left=�6N   6�   �� 5<      -   5        $��votos��5<      $��
10 10 0 10q�6,   $	��184��6A   $
��Left=�6M   6�   �� 5=      -   5        $��registerNewParty��5=      $��
10 10 0 10q�65   $	��184��6I   $
��Left=�6U   $��20��6p   $��Añadir nuevo partido��6|   6�   �� 5?      -   5        $��registroPartidos��5?      $	��184��66   $	��100��6B   $
��Left=�6O   $��
10 10 0 10q�6j   5@      5A      �� 5B      -   5        $��Actualizar datos��5B      $��Center=�60   $��16(�6M   $
��Bold"�6[   $��	Condensed&�6m   $��2��6�   $��10q�6�   $��4��6�   $	��174��6�   6�   5C      5D      5E      