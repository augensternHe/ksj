close all
clear
clc



file_list = dir('./raw_data/*.txt');

fluoro_db = struct([]);

for i = 1 : numel(file_list)
    
    fluoro_db(i).name = erase(file_list(i).name, ".txt");
    fluoro_db(i).data = importfile(fullfile(file_list(i).name));

end

save("fluoro_db.mat", "fluoro_db");


%%
function output = importfile(filename, dataLines)

if nargin < 2
    dataLines = [1, Inf];
end
opts = delimitedTextImportOptions("NumVariables", 2);
opts.DataLines = dataLines;
opts.Delimiter = "\t";
opts.VariableNames = ["VarName1", "VarName2"];
opts.VariableTypes = ["double", "double"];
opts.ExtraColumnsRule = "ignore";
opts.EmptyLineRule = "read";

output = readtable(filename, opts);
output = table2array(output);

end