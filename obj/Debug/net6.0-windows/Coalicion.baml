   M S B A M L   `   `   `  ����E  APactometro, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null'clr-namespace:Pactometro
Pactometro  5   	       Pactometro.Coalicion   5   	   W SPresentationCore, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35H 9http://schemas.microsoft.com/winfx/2006/xaml/presentation      5   	   R NSystem.Xaml, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   *localclr-namespace:Pactometro     5   	     � Title$  	Coalicion��5   	   $	��450��6   $	��800��6(   $	��450��5	   	   $	��800��5
   	    � Icon 5      �� 5   
    �� 	UriSource$> 8pack://application:,,,/Pactometro;component/alianzas.ico?�6   .��5      �  � RowDefinitions 5   
   �� 5      $
��Auto �6   �� 5      $��* �6   �� 5      $��* �6    � ColumnDefinitions 5   
   �� 5      $��* �6   �� 5      $��* �6   .+�5   
   �� $$��Formación gobierno coalición��6   $��Center=�68   $��	30 10 0 0q�6U   $��30(�6h   $
��Bold"�6v   $��	Condensed&�6�   $��2��6�   �� 5   
   -   5        $��infoPartidos��5       $��1��6    �� SelectionMode$ Single=�5       J� 	AllowDrop$
 True��5"      $��
10 10 60 0q�5#      ��5      �� *
Partidos+ �� View 5$      �� 5%      .P�5&      �� $��PARTIDO��6%    �� Width$
 auto��66   	 �� DisplayMemberBinding	 6C   �� *Nombre+�� 5'      $��VOTOS��6%   $
 auto��64   	 6A   �� *Votos+5(      5*      �� 5+   
   -   5        $��=>��5+      $��1��6   $��2��6+   $��50��6?   $	��100��6`   $��Center=�6l   �� 5,   
   -   5        $��partidosCoalicion��5,   0   $��1��6   $��1��6    $
 True��5.      $��60 10 10 0 q�50      ��5-      �� *PartidosCoalicion+ 51      �� 52      .P�53      �� $��PARTIDO��6%   $
 auto��6^   	 66   �� *Nombre+6^   �� 54      $��VOTOS��6%   $
 auto��6[   	 64   �� *Votos+6[   55      57      �� 58   
   -   5        $��2��58      $��2��6   $	��200��62   $��40��6>   $��Formar gobierno��6J   6d   59      5:      