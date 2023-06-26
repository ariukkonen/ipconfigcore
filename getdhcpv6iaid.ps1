$guid=$args[0]

get-itempropertyvalue "HKLM:System\CurrentControlSet\services\TCPIP6\Parameters\interfaces\{$guid}" -name "dhcpv6iaid"