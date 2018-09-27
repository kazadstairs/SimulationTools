

HowManyNormals <- c(1:100)

MaxOfNNorms <- function(Nnorms)
{
  stdNormals <- c(1:Nnorms);
  stdNormals <- lapply(c(1:Nnorms),function(x) stdNormals[x]=rnorm(10000,mean=1,sd=0.1))
  MaxN12 <- do.call(pmax,stdNormals)
}

means <- sapply(HowManyNormals,FUN=function(x) mean(MaxOfNNorms(x)))
vars <- sapply(HowManyNormals,FUN=function(x) var(MaxOfNNorms(x)))
varcoefficient <- vars / means
df <- data.frame(HowManyNormals,means,vars,varcoefficient)
df <- df[-1, ]

library(ggplot2)
ggplot(df,aes(x=HowManyNormals)) + geom_point(aes(y = vars, colour = "Var/Deterministic")) + 
  geom_point(aes(y = varcoefficient, colour = "Var/Mean")) + labs(x="N Parallel Machines",y="Outcome",colour="Legend")

hist(MaxN12, breaks=seq(0,6,l=50),
     freq=FALSE,col="orange",main="Histogram",
     xlab="x",ylab="f(x)",yaxs="i",xaxs="i")

xfit <- seq(min(MaxN12), max(MaxN12), length = 100) 
yfit <- dnorm(xfit, mean = mean(MaxN12), sd = sd(MaxN12)) 
lines(xfit, yfit, col = "black", lwd = 2)

abline(v=c(mean(MaxN12),mean(MaxN12)+sd(MaxN12),mean(MaxN12)-sd(MaxN12)))

######################################################################

