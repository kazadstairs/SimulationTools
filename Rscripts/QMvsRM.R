#SCHEDNAMES <- c("Rolling Machine Assignment", "Random", "GreedyLoadBalancing")
#INSTANCES <- c("Pinedo",
#               "30j-15r-4m.ms",
#               "30j-15r-8m.ms",
#               "30j-30r-4m.ms",
#               "30j-30r-4m.ms",
#               "30j-75r-4m.ms",
#               "30j-75r-4m.ms",
#               "100j-50r-6m.ms",
#               "100j-50r-12m.ms",
#               "100j-100r-6m.ms",
#              "100j-100r-12m.ms",
#              "100j-250r-6m.ms",
#               "100j-250r-12m.ms")
NRUNS <- "100"

#RM.ID <- 1
#QM.ID <- 2

###
########################## Libraries #########################################
###
installNeededPackages <- function()
{
  install.packages("ggplot2")
  install.packages("rlang")
  install.packages("plyr")
  install.packages("dplyr")
  install.packages("reshape")
  library(ggplot2)
  library(plyr)
  library(dplyr)
  library(reshape)
  library(rlang)
  show("If no warnings shown, installation successfull.")
}

library(ggplot2)
library(plyr)
library(dplyr)
library(reshape)
library(rlang)


###
########################## FUNCTIONS #########################################
###
GetPathToOutput <- function(InstanceName,ScheduleName,Nruns,FileType){
	# BasePath <- "C:\\Users\\Gebruiker\\Documents\\UU\\MSc Thesis\\Code\\OutPut\\" #laptop
  BasePath <- "C:\\Users\\3496724\\Source\\Repos\\SimulationTools\\Results\\RMs\\" #UU pc
	PathComponents <- c(BasePath,
				  "Instance_", InstanceName,
				  "_Schedule_", ScheduleName,
				  "_Runs_", Nruns,
				  "_",FileType,
				  ".txt")
	FilePath <- paste(PathComponents, collapse="")
	return(FilePath)
}


GetSimSettings <- function(InstanceName,ScheduleName,Nruns){
	path <- GetPathToOutput(InstanceName,ScheduleName,Nruns,"SimSettings")
	return(read.csv2(file = path,header = FALSE))
}
GetQMs <- function(InstanceName,ScheduleName,Nruns){
	path <- GetPathToOutput(InstanceName,ScheduleName,Nruns,"QMs")
	return(read.csv2(file = path, header = FALSE))
}

GetInstanceName <- function(){
	return(SimSettings[1,])
}

GetScheduleName <- function(){
	return(SimSettings[2,])
}

GetRMName <- function(){
	f <- levels(GetSimSettings(INSTANCE,SCHEDNAMES[1],NRUNS)[2+2*RM.ID,])[6]
	return(f)
}

QMNames <- c("Cmax","Linear Start Delay","Start Punctuality","TODO: Completion Punctuality")
GetQMName <- function(){
	return(QMNames[QM.ID])
}

GetRM <- function(SimSettings){
	f <- SimSettings[3+2*RM.ID,]
	return( as.numeric(levels(f))[f] )
}

GetQM <- function(QMData){
	return(QMData[,1+QM.ID])
}



GetCmaxColumn <- function(QMData){
	return(QMData[,2])
}

GetLinearDelayColumn <- function(){
	return(QMData[,3])
}

GetStartPunctualityColumn <- function(){
	return(QMData[,4])
}

GetCompletionPunctualiyColumn <- function(){
	return(QMData[,5])
}
####




BuildDfFor <- function(InstanceName,ScheduleName,Nruns){ 
	rmvals <- GetRM(GetSimSettings(InstanceName,ScheduleName,Nruns))
	cmax <- GetQM(GetQMs(InstanceName,ScheduleName,Nruns))
	df <- data.frame( RM = rmvals, Cmax = cmax)
	df.summary <- df %>% group_by(RM) %>% summarize(ymin = mean(cmax) - sd(cmax), ymax = mean(cmax) + sd(cmax),ymean = mean(cmax))
	df.summary$PointLabel <- abbreviate(ScheduleName)
	return( df.summary )
}

# build data frame:
BuildPlot <- function(cScheduleNames,InstanceName,Nruns)
{

	
	##
	
	dflist <- lapply(cScheduleNames,FUN = BuildDfFor, InstanceName = InstanceName, Nruns = Nruns)
	dfplotinfo <- rbind.fill(dflist)
	
	return (dfplotinfo)
}

MakeQuantilePlot <-  function(string.RM,double.upperQ,type)
{
  if(type == "absolute")
  {
    p <- MakeAbsoluteQuantileDifPlot(string.RM,double.upperQ)
  }
  else if(type == "relative")
  {
    p <- MakeRelativeQuantileDifPlot(string.RM,double.upperQ)
  }
  else
  {
    show("Type parameter unrecognized. Must be absolute or relative")
  }
  
  fileName <- paste(PATH,"_",Sys.Date(),"_",string.RM,"_vs_",type,"_",double.upperQ,"_quantile",".pdf",sep="")
  show(paste("Saving to:",fileName))
  ggsave(filename = fileName,plot = p)
  
}

MakeAbsoluteQuantileDifPlot <- function(string.RM,double.upperQ)
{
  library(dplyr)
  RMsym <- rlang::sym(string.RM)
  string.QM <- "Cmax"
  QMsym <- rlang::sym(string.QM)
  string.RelativeTo <- "DetCmax"
  RELsym <- rlang::sym(string.RelativeTo)
  
  myDF.plot <- myDF %>% 
    group_by(Distribution.Type,Instance.Name,Schedule.AssignType,Schedule.StartTimeType) %>% 
    summarize(RM = mean(!!RMsym),QM = quantile((!!QMsym),double.upperQ)-mean(!!RELsym))
  
  Xvals <- myDF.plot[,"RM"][[1]]
  Yvals <- myDF.plot[,"QM"][[1]] #[,""] gets the single tibble column. THen [[1]] gets the first element: The vector
  Srho <- cor.test(x=Xvals,y=Yvals,method = "spearman")
  
  
  p <- ggplot(myDF.plot,aes(x=RM,y=QM,colour=Schedule.AssignType,shape=Schedule.StartTimeType)) 
  p <- p + geom_point() 
  p <- p + scale_x_continuous(expand = c(0, 0),limits = c(0,1.1*max(myDF.plot$RM))) 
  p <- p + scale_y_continuous(expand = c(0, 0),limits= c(0,1.1*max(myDF.plot$QM)))
  p <- p + xlab(string.RM) + ylab(paste(100*double.upperQ,"% quantile Cmax - mean(Cmax)"))
  p <- p + theme(legend.position = "top") + ggtitle(paste(100*double.upperQ,"% quantile - 50% quantile. Spearman = ",Srho$estimate))
  show(p)
  
}

MakeRelativeQuantileDifPlot <- function(string.RM,double.upperQ)
{
  library(dplyr)
  RMsym <- rlang::sym(string.RM)
  string.QM <- "Cmax"
  QMsym <- rlang::sym(string.QM)
  string.RelativeTo <- "DetCmax"
  RELsym <- rlang::sym(string.RelativeTo)
  
  myDF.plot <- myDF %>% 
    group_by(Distribution.Type,Instance.Name,Schedule.AssignType,Schedule.StartTimeType) %>% 
    summarize(RM = mean(!!RMsym),QM = (quantile((!!QMsym),double.upperQ)/mean(!!RELsym))-1)
  
  Xvals <- myDF.plot[,"RM"][[1]]
  Yvals <- myDF.plot[,"QM"][[1]] #[,""] gets the single tibble column. THen [[1]] gets the first element: The vector
  Srho <- cor.test(x=Xvals,y=Yvals,method = "spearman")
  
  
  p <- ggplot(myDF.plot,aes(x=RM,y=QM,colour=Schedule.AssignType,shape=Schedule.StartTimeType,label=Instance.Name)) 
  #p <- p + geom_text(aes(label=Instance.Name),hjust=0, vjust=0)
  p <- p + geom_point() 
  p <- p + scale_x_continuous(expand = c(0, 0),limits = c(0,1.1*max(myDF.plot$RM))) 
  p <- p + scale_y_continuous(expand = c(0, 0),limits= c(0,1.1*max(myDF.plot$QM)))
  p <- p + xlab(string.RM) + ylab(paste(100*double.upperQ,"% quantile Cmax / DetCmax - 1"))
  p <- p + theme(legend.position = "top") + ggtitle(paste(100*double.upperQ,"% quantile / 50% quantile. Spearman = ",Srho$estimate))
  show(p)
  
}

MakeRelativeSDPlot <- function(string.RM)
{
  library(dplyr)
  RMsym <- rlang::sym(string.RM)
  string.QM <- "Cmax"
  QMsym <- rlang::sym(string.QM)
  string.RelativeTo <- "DetCmax"
  RELsym <- rlang::sym(string.RelativeTo)
  
  myDF.plot <- myDF %>% 
    group_by(Distribution.Type,Instance.Name,Schedule.AssignType,Schedule.StartTimeType) %>% 
    summarize(RM = mean(!!RMsym),QM = (sd(!!QMsym)/mean(!!RELsym)))
  
  View(myDF.plot)
  Xvals <- myDF.plot[,"RM"][[1]]
  Yvals <- myDF.plot[,"QM"][[1]] #[,""] gets the single tibble column. THen [[1]] gets the first element: The vector
  Srho <- cor.test(x=Xvals,y=Yvals,method = "spearman")
  
  
  p <- ggplot(myDF.plot,aes(x=RM,y=QM,colour=Schedule.AssignType,shape=Schedule.StartTimeType,label=Instance.Name)) 
  #p <- p + geom_text(aes(label=Instance.Name),hjust=0, vjust=0)
  p <- p + geom_point() 
  p <- p + scale_x_continuous(expand = c(0, 0),limits = c(0,1.1*max(myDF.plot$RM))) 
  p <- p + scale_y_continuous(expand = c(0, 0),limits= c(0,1.1*max(myDF.plot$QM)))
  p <- p + xlab(string.RM) + ylab("sd(Cmax)/Cmax")
  p <- p + theme(legend.position = "top") + ggtitle(paste("sd(Cmax)/Cmax. Spearman = ",Srho$estimate))
  show(p)
  
}


MakePlot <- function(string.RM, string.QM)
{
  RMsym <- sym(string.RM)
  QMsym <- sym(string.QM)
  #RMsym <- sym("FS")
  #QMsym <- sym("Cmax")
  library(ggplot2)
  #library(tidyverse)
  
  myDF.plot <- myDF %>% 
    group_by(Distribution.Type,Instance.Name,Schedule.AssignType,Schedule.StartTimeType) %>% 
    summarize(RM = mean(!!RMsym),QMsd = sd(!!QMsym),QM=mean(!!QMsym))
  
  show(myDF.plot)
  # names(myDF.plot)[3] <- string.RM
  #  names(myDF.plot)[4] <- paste(string.QM,"sd",sep="")
  #  names(myDF.plot)[5] <- string.QM
  Xvals <- myDF.plot[,"RM"][[1]]
  Yvals <- myDF.plot[,"QM"][[1]] #[,""] gets the single tibble column. THen [[1]] gets the first element: The vector
  Srho <- cor.test(x=Xvals,y=Yvals,method = "spearman")

  p <- ggplot(myDF.plot,aes(x=RM,y=QM,colour=Schedule.AssignType,shape=Schedule.AssignType)) 
  p <- p + geom_point() 
  p <- p + geom_errorbar(aes(ymin=QM-QMsd,ymax=QM+QMsd))
  p <- p + scale_x_continuous(expand = c(0, 0),limits = c(0,1.1*max(myDF.plot$RM))) 
  p <- p + scale_y_continuous(expand = c(0, 0),limits= c(0,1.1*max(myDF.plot$QM)))
  p <- p + xlab(string.RM) + ylab(string.QM)
  p <- p + theme(legend.position = "top") + ggtitle(paste("Spearman = ",Srho$estimate))
  
  fileName <- paste(PATH,"_",Sys.Date(),"_",string.RM,"_vs_",string.QM,".pdf",sep="")
  ggsave(filename = fileName,plot = p)
  
  print(p)
  
}
MakePlot.WithRange <- function(string.RM, string.QM,xRange,yRange)
{
  RMsym <- sym(string.RM)
  QMsym <- sym(string.QM)
  #RMsym <- sym("FS")
  #QMsym <- sym("Cmax")
  library(ggplot2)
  #library(tidyverse)
  
  myDF.plot <- myDF %>% 
    group_by(Distribution.Type,Instance.Name,Schedule.AssignType,Schedule.StartTimeType) %>% 
    summarize(RM = mean(!!RMsym),QMsd = sd(!!QMsym),QM=mean(!!QMsym))
  
  # names(myDF.plot)[3] <- string.RM
  #  names(myDF.plot)[4] <- paste(string.QM,"sd",sep="")
  #  names(myDF.plot)[5] <- string.QM
  
  p <- ggplot(myDF.plot,aes(x=RM,y=QM,colour=Schedule.AssignType,shape=Schedule.AssignType)) 
  p <- p + geom_point() 
  p <- p + geom_errorbar(aes(ymin=QM-QMsd,ymax=QM+QMsd))
  p <- p + scale_x_continuous(expand = c(0, 0),limits = c(0,xRange)) 
  p <- p + scale_y_continuous(expand = c(0, 0),limits= c(0,yRange))
  p <- p + xlab(paste("mean ",string.RM)) + ylab(string.QM)
  p <- p + theme(legend.position = "top")
  
  fileName <- paste(PATH,"_",Sys.Date(),"_",string.RM,"_vs_",string.QM,".pdf",sep="")
  ggsave(filename = fileName,plot = p)
  
  print(p)
  
}

MakeAllPlots <- function()
{
  RMs <- c("FS","wFS","UFS","TS","wTS","BTS","UTS","DetCmax","NormalApproxCmax")
  QMs <- c("Cmax","LinearStartDelay","Start.Punctuality","Finish.Punctuality","DetCmax")
  
  for(Rm in RMs)
  {
    for(Qm in QMs)
    {
      MakePlot(Rm,Qm)
    }
    
    MakeQuantilePlot(Rm,0.95,"absolute")
    MakeQuantilePlot(Rm,0.95,"relative")
    
  }
  
}

Make.Different.PI.and.Distros.Plot <- function()
{
  ggplot(aes(y = Start.Punctuality, x = Distribution.Type, fill = Instance.Name), data = myDF) + geom_boxplot()
}
#
#
######## ACTUAL WORK ##########################################
#
#


#plot.df <- BuildPlot(SCHEDNAMES,INSTANCE,NRUNS)
#errorBarWidth <- max(plot.df$RM) / (4 * length(unique(plot.df$RM)))
#p <- ggplot(plot.df, aes(x = RM, y = ymean, label= PointLabel)) + geom_point(size = 2) + geom_errorbar(aes(ymin = ymin, ymax = ymax, width = errorBarWidth))+geom_text(aes(label=PointLabel),hjust=0, vjust=0)
#p + labs(x = paste(c("Schedule ",abbreviate(GetRMName())," score"),collapse=''), y = GetQMName())

########### plot from one big data file ############

UUPATH <- "C:/Users/3496724/Source/Repos/SimulationTools/Results/RMs/allresults.txt"
LAPTOPPATH <- "C:/Users/Gebruiker/Documents/UU/MSc Thesis/Code/Simulation/SimulationTools/Results/RMs/allresults.txt"
PATH <- LAPTOPPATH
myDF <- read.csv2(LAPTOPPATH)
myDF <- subset(myDF,myDF$Distribution.Type == "N(p,0.3p)")
myDF <- myDF[,1:20]
MakePlot(string.RM = "DetCmax",string.QM = "Cmax")
MakePlot.WithRange("TS","Cmax",500,500)
MakeAllPlots()
MakeQuantilePlot("BTS",0.95,type="relative")
Make.Different.PI.and.Distros.Plot()

############################
#For debugging
myDF.plot <- myDF %>% 
  group_by(Distribution.Type,Instance.Name,Schedule.AssignType,Schedule.StartTimeType) %>% 
  summarize(SS1=mean(ScheduledStartTime1),Delay=mean(RealisedStartTime1-ScheduledStartTime1))

PlotSchedStartvsDelay <- function(string.Instance,string.AssignType)
{
  library(reshape)
  myDF <- read.csv2(PATH)
  myDF <- subset(myDF,grepl(string.Instance,Instance.Name))
  myDF <- subset(myDF,grepl(string.AssignType,Schedule.AssignType))
  melted.myDF <- melt(myDF,id=names(myDF)[1:12])
  my.melted.DF.plot <- melted.myDF %>% 
    group_by(Instance.Name,Schedule.AssignType,Schedule.StartTimeType,variable) %>% 
    summarize(mval = mean(value),sdval = sd(value))
  View(my.melted.DF.plot)
  melted2.df.plot <- subset(my.melted.DF.plot,grepl("Scheduled", variable, fixed=TRUE))
  xval <- subset(my.melted.DF.plot,grepl("Scheduled", variable, fixed=TRUE))$mval
  yval <- subset(my.melted.DF.plot,grepl("RealisedStartTime", variable, fixed=TRUE))$mval - xval
  yvalsd <- subset(my.melted.DF.plot,grepl("RealisedStartTime", variable, fixed=TRUE))$sdval
  df.plot <- data.frame(melted2.df.plot,yval,yvalsd)
  p <- ggplot(df.plot,aes(x=mval,y=yval,group=interaction(Schedule.AssignType,Instance.Name),colour=Schedule.AssignType,shape=Instance.Name))
  p + geom_point()
  p <- p + geom_errorbar(aes(ymin=yval-yvalsd,ymax=yval+yvalsd))
  p <- p + xlab(paste("Jobstarttime")) + ylab("Delay")
  p <- p + theme(legend.position = "top")
  show(p)
}
   


starttimes <- subset(my.melted.DF.plot,grepl("Scheduled",variable,fixed = TRUE))$mval
cbind(my.melted.DF.plot,starttimes)

ggplot(myDF.plot,aes(x=SS1,y=Delay,colour = Schedule.AssignType,shape=Schedule.AssignType)) + geom_point()

p <- ggplot(myDF.plot,aes(x=FS,y=Cmax,colour=Schedule.StartTimeType,shape=Schedule.StartTimeType)) 
p <- p + geom_point() 
p <- p + geom_errorbar(aes(ymin=Cmax-Cmaxsd,ymax=Cmax+Cmaxsd))
p <- p + scale_x_continuous(expand = c(0, 0),limits = c(0,2000)) 
p <- p + scale_y_continuous(expand = c(0, 0),limits= c(0,500))

p

############ Sampling tests

Zs <- rnorm(10000)
mean(Zs)
LNmean <- 5
LNvar <- 10^2
mu <- log(LNmean^2 / sqrt(LNvar + LNmean^2))
sigma <- sqrt(log(1+LNvar/LNmean^2))
Xs <- exp(mu + sigma*Zs)
mean(Xs)
sd(Xs)

#MakeAllPlots()



