SCHEDNAMES <- c("Rolling Machine Assignment", "Random", "GreedyLoadBalancing")
INSTANCES <- c("Pinedo",
               "30j-15r-4m.ms",
               "30j-15r-8m.ms",
               "30j-30r-4m.ms",
               "30j-30r-4m.ms",
               "30j-75r-4m.ms",
               "30j-75r-4m.ms",
               "100j-50r-6m.ms",
               "100j-50r-12m.ms",
               "100j-100r-6m.ms",
               "100j-100r-12m.ms",
               "100j-250r-6m.ms",
               "100j-250r-12m.ms")
NRUNS <- "1000"

RM.ID <- 1
QM.ID <- 2

###
########################## Libraries #########################################
###
library(ggplot2)
library(plyr)
library(dplyr)

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
  
  # names(myDF.plot)[3] <- string.RM
  #  names(myDF.plot)[4] <- paste(string.QM,"sd",sep="")
  #  names(myDF.plot)[5] <- string.QM
  
  p <- ggplot(myDF.plot,aes(x=RM,y=QM,colour=Distribution.Type,shape=Distribution.Type)) 
  p <- p + geom_point() 
  p <- p + geom_errorbar(aes(ymin=QM-QMsd,ymax=QM+QMsd))
  p <- p + scale_x_continuous(expand = c(0, 0),limits = c(0,1.1*max(myDF.plot$RM))) 
  p <- p + scale_y_continuous(expand = c(0, 0),limits= c(0,1.1*max(myDF.plot$QM)))
  p <- p + xlab(string.RM) + ylab(string.QM)
  p <- p + theme(legend.position = "top")
  
  fileName <- paste("C:\\Users\\3496724\\Source\\Repos\\SimulationTools\\Results\\RMs\\",string.RM,"_vs_",string.QM,".pdf",sep="")
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
  
  p <- ggplot(myDF.plot,aes(x=RM,y=QM,colour=Distribution.Type,shape=Distribution.Type)) 
  p <- p + geom_point() 
  p <- p + geom_errorbar(aes(ymin=QM-QMsd,ymax=QM+QMsd))
  p <- p + scale_x_continuous(expand = c(0, 0),limits = c(0,xRange)) 
  p <- p + scale_y_continuous(expand = c(0, 0),limits= c(0,yRange))
  p <- p + xlab(string.RM) + ylab(string.QM)
  p <- p + theme(legend.position = "top")
  
  fileName <- paste("C:\\Users\\3496724\\Source\\Repos\\SimulationTools\\Results\\RMs\\",string.RM,"_vs_",string.QM,".pdf",sep="")
  ggsave(filename = fileName,plot = p)
  
  print(p)
  
}

Plot.SchedStart.vs.RealisedStart <- function()
{
  
  
}

MakeAllPlots <- function()
{
  RMs <- c("FS","BFS","UFS","wFS")
  QMs <- c("Cmax","LinearStartDelay","Start.Punctuality","Finish.Punctuality")
  
  for(Rm in RMs)
  {
    for(Qm in QMs)
    {
      MakePlot(Rm,Qm)
    }
    
  }
  
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
LAPTOPPATH <- "C:/Users/Gebruiker/Documents/UU/MSc Thesis/Code/Simulation/SimulationTools/Results/RMs/allresults.txt"
myDF <- read.csv2(LAPTOPPATH)
MakePlot.WithRange("FS","Cmax",1000,1000)
MakeAllPlots()


############################
#For debugging
myDF.plot <- myDF %>% 
  group_by(Distribution.Type,Instance.Name,Schedule.AssignType,Schedule.StartTimeType) %>% 
  summarize(SS1=mean(ScheduledStartTime1),Delay=mean(RealisedStartTime1-ScheduledStartTime1))

PlotSchedStartvsDelay <- function(string.Instance,string.AssignType)
{
  myDF <- read.csv2("C:/Users/3496724/Source/Repos/SimulationTools/Results/RMs/allresults.txt")
  myDF <- subset(myDF,grepl(string.Instance,Instance.Name))
  myDF <- subset(myDF,grepl(string.AssignType,Schedule.AssignType))
  melted.myDF <- melt(myDF,id=names(myDF)[1:12])
  my.melted.DF.plot <- melted.myDF %>% 
    group_by(Instance.Name,Schedule.AssignType,Schedule.StartTimeType,variable) %>% 
    summarize(mval = mean(value),sdval = sd(value))
  melted2.df.plot <- subset(my.melted.DF.plot,grepl("Scheduled", variable, fixed=TRUE))
  xval <- subset(my.melted.DF.plot,grepl("Scheduled", variable, fixed=TRUE))$mval
  yval <- subset(my.melted.DF.plot,grepl("RealisedStartTime", variable, fixed=TRUE))$mval - xval
  yvalsd <- subset(my.melted.DF.plot,grepl("RealisedStartTime", variable, fixed=TRUE))$sdval
  df.plot <- data.frame(melted2.df.plot,yval,yvalsd)
  p <- ggplot(df.plot,aes(x=mval,y=yval,group=interaction(Schedule.AssignType,Instance.Name),colour=Schedule.AssignType,shape=Instance.Name))
  p + geom_point()
  
}
   


starttimes <- subset(my.melted.DF.plot,grepl("Scheduled",variable,fixed = TRUE))$mval
cbind(my.melted.DF.plot,starttimes)

ggplot(myDF.plot,aes(x=SS1,y=Delay,colour = Schedule.AssignType,shape=Schedule.AssignType)) + geom_point()

#p <- ggplot(myDF.plot,aes(x=FS,y=Cmax,colour=Schedule.StartTimeType,shape=Schedule.StartTimeType)) 
#p <- p + geom_point() 
#p <- p + geom_errorbar(aes(ymin=Cmax-Cmaxsd,ymax=Cmax+Cmaxsd))
#p <- p + scale_x_continuous(expand = c(0, 0),limits = c(0,2000)) 
#p <- p + scale_y_continuous(expand = c(0, 0),limits= c(0,500))

#p

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



